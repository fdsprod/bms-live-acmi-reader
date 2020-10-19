using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using BMS.MemoryMap.Acmi;

namespace BMS.MemoryMap
{
    class Program
    {
        private static readonly string _acmiFolder = @"D:\Falcon BMS 4.34\User\Acmi";
        private static readonly string _parseFolder = Path.Combine(_acmiFolder, "Parsing");
        private static readonly Dictionary<Type, int> _typeSize = new Dictionary<Type, int>();
        private static readonly List<Entity> _entities = new List<Entity>();
        private static readonly Queue<string> _queue = new Queue<string>();

        private static FileSystemWatcher _fltWatcher;
        private static FileSystemWatcher _parseWatcher;

        private static Thread _workerThread;
        private static DateTime _lastUpdate;

        static void Main(string[] args)
        {
            if (!Directory.Exists(_parseFolder))
            {
                Directory.CreateDirectory(_parseFolder);
            }

            foreach (var file in Directory.GetFiles(_parseFolder))
            {
                File.Delete(file);
            }
            
            QueueAllFiles();
            CreateFileWatchers();
            CreateWorkerThread();

            while (true)
            {
                Console.Clear();

                lock (_entities)
                {
                    Console.WriteLine($"Last Updated {(DateTime.Now - _lastUpdate).TotalSeconds:0.} seconds ago");

                    foreach (var entity in _entities.Take(15))
                    {
                        if (string.IsNullOrEmpty(entity.Name))
                        {
                            continue;
                        }

                        Console.WriteLine($"Entity \"{entity.Name}\" Latitude: {entity.Latitude} Longitude: {entity.Longitude} Altitude: {entity.Altitude}");
                    }
                }

                Thread.Sleep(1000);
            }
        }

        private static void CreateWorkerThread()
        {
            _workerThread = new Thread(ProcessNext);
            _workerThread.Start();
        }

        private static void CreateFileWatchers()
        {
            _fltWatcher = new FileSystemWatcher(_acmiFolder);
            _fltWatcher.Filter = "*.flt";
            _fltWatcher.Created += OnFltFileCreated;
            _fltWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastWrite;
            _fltWatcher.EnableRaisingEvents = true;

            _fltWatcher = new FileSystemWatcher(_parseFolder);
            _fltWatcher.Filter = "*.parse";
            _fltWatcher.Renamed += OnParseFileRenamed;
            _fltWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastWrite;
            _fltWatcher.EnableRaisingEvents = true;
        }

        private static void QueueAllFiles()
        {
            var dir = new DirectoryInfo(_acmiFolder);
            var files = dir.GetFiles("*.flt")
                .OrderBy(f => f.CreationTime)
                .ToArray();

            lock (_queue)
            {
                foreach (var file in files.Take(files.Length - 1))
                {
                    lock (_queue)
                    {
                        _queue.Enqueue(file.FullName);
                    }
                }
            }

            _lastUpdate = DateTime.Now;
        }

        private static void ProcessNext()
        {
            while (true)
            {
                string fileName = null;

                lock (_queue)
                {
                    if (_queue.Count > 0)
                    {
                        fileName = _queue.Dequeue();
                    }
                }

                if (fileName != null)
                {
                    var positionRecords = new List<AcmiPositionRecordBase>();
                    var callsignRecords = new List<AcmiCallsignRecord>();

                    var buffer = File.ReadAllBytes(fileName);

                    try
                    {
                        ParseFlt(buffer, positionRecords, callsignRecords);
                        ProcessEntities(positionRecords, callsignRecords);
                    }
                    catch (Exception e)
                    {
                        // Sometimes the file isn't fully written or ready, let's skip it and try again later.   
                        Thread.Sleep(50);
                        continue;
                    }

                    File.Delete(fileName);

                    _lastUpdate = DateTime.Now;
                }

                Thread.Sleep(100);
            }
        }

        private static void OnParseFileRenamed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Renamed)
            {
                return;
            }

            lock (_queue)
            {
                _queue.Enqueue(e.FullPath);
            }
        }

        private static void OnFltFileCreated(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Created)
            {
                return;
            }

            CopyToParseFolder(e.FullPath);
        }

        private static string CopyToParseFolder(string fullPath)
        {
            var fileName = Path.GetFileName(fullPath);
            var parseFile = Path.Combine(_parseFolder, fileName);

            File.Copy(fullPath, parseFile);
            File.Move(parseFile, parseFile + ".parse");

            return parseFile;
        }

        private static void ProcessEntities(List<AcmiPositionRecordBase> positionRecords, List<AcmiCallsignRecord> callsignRecords)
        {
            foreach (var record in positionRecords)
            {
                if ((record.EntityFlag & EntityFlags.Chaff) == EntityFlags.Chaff ||
                    (record.EntityFlag & EntityFlags.Flare) == EntityFlags.Flare ||
                    (record.EntityFlag & EntityFlags.Missile) == EntityFlags.Missile)
                {
                    continue;
                }

                var isFeature = (record.EntityFlag & EntityFlags.Feature) == EntityFlags.Feature;

                if (isFeature)
                {
                    continue;
                }

                Entity entity;

                lock (_entities)
                {
                    entity = _entities.FirstOrDefault(e => e.Id == record.UniqueId);

                    if (entity == null)
                    {
                        entity = new Entity
                        {
                            Id = record.UniqueId,
                            ObjectType = record.ObjectType,
                            Flag = record.EntityFlag,
                        };

                        _entities.Add(entity);

                        var callsignRecord = callsignRecords[_entities.Count - 1];

                        entity.Name = callsignRecord.Label;
                        entity.Team = callsignRecord.TeamColor;
                    }
                }

                if (!(entity.LastUpdateTime < record.Header.Time))
                {
                    continue;
                }

                CalculateLatLong(entity, record);

                entity.Altitude = -entity.Z;
            }
        }

        private static void CalculateLatLong(Entity entity, AcmiPositionRecordBase record)
        {
            //Kuwait
            const float BaseLongitude = 42.11698f;
            const float BaseLatitude = 25.09686f;
            const float FT_PER_DEGREE = 365221.8846f;
            const float RTD = 57.2957795f; //Radians to Degrees
            const float DTR = 0.01745329f; //Degrees to Radians
            const float EARTH_RADIUS_FEET = 20925700f;

            entity.X = record.X;
            entity.Y = record.Y;
            entity.Z = record.Z;

            var StartCoord_Lat = BaseLatitude;
            var StartCoord_Long = BaseLongitude;

            //Latitude in Radians
            var latitude = (StartCoord_Lat * FT_PER_DEGREE + entity.X) / EARTH_RADIUS_FEET;

            //Cosine of Latitude
            var cosLat = (float) Math.Cos(latitude);

            //Longitude in Radians
            var longitude = ((StartCoord_Long * DTR * EARTH_RADIUS_FEET * cosLat) + entity.Y) / (EARTH_RADIUS_FEET * cosLat);

            //Converting to Degrees
            entity.Latitude = latitude * RTD;
            entity.Longitude = longitude * RTD;
        }

        private static void ParseFlt(byte[] buffer, List<AcmiPositionRecordBase> positionRecords, List<AcmiCallsignRecord> callsignRecords)
        {
            var offset = 0;

            AcmiTapeHeader tapeHdr;

            while (offset < buffer.Length)
            {
                var header = ByteArrayToStructure<AcmiRecHeader>(buffer, ref offset);
                var type = (AcmiHeaderType) header.Type;

                //Console.WriteLine($"Read type: {type}");

                switch (type)
                {
                    case AcmiHeaderType.AcmiRecGenPosition:
                    case AcmiHeaderType.AcmiRecMissilePosition:
                    case AcmiHeaderType.AcmiRecChaffPosition:
                    case AcmiHeaderType.AcmiRecFlarePosition:
                    case AcmiHeaderType.AcmiRecAircraftPosition:
                    {
                        var data = ByteArrayToStructure<AcmiGenPositionData>(buffer, ref offset);

                        AcmiPositionRecordBase record;

                        if (type == AcmiHeaderType.AcmiRecAircraftPosition)
                        {
                            var radarTarget = ByteArrayToStructure<int>(buffer, ref offset);

                            record = new AcmiAircraftPositionRecord
                            {
                                RadarTarget = radarTarget
                            };
                        }
                        else
                        {
                            record = new AcmiGenPositionRecord();
                        }

                        record.Header = header;
                        record.UniqueId = data.UniqueId;
                        record.ObjectType = data.ObjectType;
                        record.Pitch = data.Pitch;
                        record.Roll = data.Roll;
                        record.Yaw = data.Yaw;
                        record.X = data.X;
                        record.Y = data.Y;
                        record.Z = data.Z;

                        record.EntityFlag = type switch
                        {
                            AcmiHeaderType.AcmiRecMissilePosition => EntityFlags.Missile,
                            AcmiHeaderType.AcmiRecChaffPosition => EntityFlags.Chaff,
                            AcmiHeaderType.AcmiRecFlarePosition => EntityFlags.Flare,
                            AcmiHeaderType.AcmiRecAircraftPosition => EntityFlags.Aircraft,
                            _ => record.EntityFlag
                        };

                        positionRecords.Add(record);

                        break;
                    }
                    case AcmiHeaderType.AcmiRecFeaturePosition:
                    {
                        var data = ByteArrayToStructure<AcmiFeaturePositionData>(buffer, ref offset);
                        var record = new AcmiFeaturePositionRecord
                        {
                            Header = header,
                            UniqueId = data.UniqueId,
                            ObjectType = data.Type,
                            LeadUniqueId = data.LeadUniqueId,
                            SpecialFlags = data.SpecialFlags,
                            Slot = data.Slot,
                            Pitch = data.Pitch,
                            Roll = data.Roll,
                            Yaw = data.Yaw,
                            X = data.X,
                            Y = data.Y,
                            Z = data.Z,
                            EntityFlag = EntityFlags.Feature
                        };

                        positionRecords.Add(record);
                        break;
                    }
                    case AcmiHeaderType.AcmiRecTracerStart:
                    {
                        offset += SizeOfType(typeof(AcmiTracerStartData));
                        //var value = ByteArrayToStructure<AcmiTracerStartData>(buffer, ref offset);
                        break;
                    }
                    case AcmiHeaderType.AcmiRecStationarySfx:
                    {
                        offset += SizeOfType(typeof(AcmiStationarySfxData));
                        //var value = ByteArrayToStructure<AcmiStationarySfxData>(buffer, ref offset);
                        break;
                    }
                    case AcmiHeaderType.AcmiRecMovingSfx:
                    {

                        offset += SizeOfType(typeof(AcmiMovingSfxData));
                        //var value = ByteArrayToStructure<AcmiMovingSfxData>(buffer, ref offset);
                        break;
                    }
                    case AcmiHeaderType.AcmiRecSwitch:
                    {
                        var value = ByteArrayToStructure<AcmiSwitchData>(buffer, ref offset);
                        break;
                    }
                    case AcmiHeaderType.AcmiRecDof:
                    {
                        var value = ByteArrayToStructure<AcmiDofData>(buffer, ref offset);
                        break;
                    }
                    case AcmiHeaderType.AcmiRecTodOffset:
                        tapeHdr.TodOffset = header.Time;
                        break;
                    case AcmiHeaderType.AcmiRecFeatureStatus:
                    {
                        var value = ByteArrayToStructure<AcmiFeatureStatusData>(buffer, ref offset);
                        break;
                    }
                    case AcmiHeaderType.AcmiCallsignList:
                    {
                        var count = ByteArrayToStructure<long>(buffer, ref offset);

                        offset += sizeof(long) * 2;

                        for (var i = 0; i < count - 1; i++)
                        {
                            var callsign = Encoding.ASCII.GetString(buffer, offset, 16)
                                .Trim('\0');
                            var teamColor = BitConverter.ToInt32(buffer, offset + 16);

                            var record = new AcmiCallsignRecord
                            {
                                Label = callsign,
                                TeamColor = teamColor
                            };

                           // Console.WriteLine($"Found Callsign: {record.Label}");

                            callsignRecords.Add(record);

                            offset += 20;
                        }

                        break;
                    }
                    case AcmiHeaderType.AcmiRecMaxTypes:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static unsafe T ByteArrayToStructure<T>(byte[] bytes, ref int offset) where T : struct
        {
            fixed (byte* ptr = &bytes[offset])
            {
                offset += SizeOfType(typeof(T));
                return (T)Marshal.PtrToStructure((IntPtr)ptr, typeof(T));
            }
        }

        private static int SizeOfType(Type type)
        {
            if (_typeSize.TryGetValue(type, out var size))
            {
                return size;
            }

            var dm = new DynamicMethod("SizeOfType", typeof(int), new Type[] { });
            var il = dm.GetILGenerator();

            il.Emit(OpCodes.Sizeof, type);
            il.Emit(OpCodes.Ret);

            size = (int) dm.Invoke(null, null);
            _typeSize[type] = size;

            return size;
        }
    }

    [Flags]
    public enum EntityFlags
    {
        Object = 0x00000000,
        Missile = 0x00000001,
        Feature = 0x00000002,
        Aircraft = 0x00000004,
        Chaff = 0x00000008,
        Flare = 0x00000010,
    }

    public class Entity
    {
        public int ObjectType
        {
            get;
            set;
        }

        public int Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public EntityFlags Flag
        {
            get;
            set;
        }

        public float LastUpdateTime
        {
            get;
            set;
        }

        public float X
        {
            get;
            set;
        }

        public float Y
        {
            get;
            set;
        }

        public float Z
        {
            get;
            set;
        }

        public int Team
        {
            get;
            set;
        }

        public float Latitude
        {
            get;
            set;
        }

        public float Longitude
        {
            get;
            set;
        }

        public float Altitude
        {
            get;
            set;
        }
    }
}

