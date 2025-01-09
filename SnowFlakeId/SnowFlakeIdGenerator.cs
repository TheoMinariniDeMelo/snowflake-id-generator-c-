using System.Reflection.Metadata.Ecma335;

namespace SnowFlakeId
{
    public class SnowFlakeIdGenerator
    {
        private readonly long Epoch = 1640995200000L;
        private const int MachineIdBits = 10;
        private const int SequenceBits = 12;

        private const long MaxMachineId = (1L << MachineIdBits) - 1;
        private const long MaxSequence = (1L << SequenceBits) - 1;

        private long _machineId;
        private long _sequence = 0L;
        private long _lastTimestamp = -1L;
        public SnowFlakeIdGenerator(long machineId)
        {
            if (machineId < 0 || machineId > MaxMachineId)
            {
                throw new ArgumentException($"Invalid machine id {machineId}");
            }
            _machineId = machineId;
        }
        public long Generate()
        {
            lock (this)
            {
                long timestamp = GetCurrentTimestamp();

                if (timestamp == _lastTimestamp)
                {
                    _sequence = (_sequence + 1) & SequenceBits;
                    timestamp = this.WaitForTheNextTimestamp(_lastTimestamp);
                }
                else
                {
                    _sequence = 0;
                }
                _lastTimestamp = timestamp;

                return (timestamp - Epoch) << (MachineIdBits + SequenceBits)
                    | (_machineId << SequenceBits)
                    | _sequence;
            }
        }
        private long GetCurrentTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        private long WaitForTheNextTimestamp(long lastTimestamp)
        {
            long timestamp = GetCurrentTimestamp();
            while (timestamp <= lastTimestamp)
            {
                timestamp = GetCurrentTimestamp();
            }
            return timestamp;
        }

    }
}
