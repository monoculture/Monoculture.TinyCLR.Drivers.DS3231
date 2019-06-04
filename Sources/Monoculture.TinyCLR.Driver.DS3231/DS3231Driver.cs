/*
 * Author: Monoculture 2019
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Native;

namespace Monoculture.TinyCLR.Drivers.DS3231
{
    public class DS3231Driver
    {
        private readonly I2cDevice _device;

        private const byte DS3231_ADD = 0x68;
        private const byte DS3231_REG_TIME = 0x00;
        private const byte DS3231_REG_ALARM_1 = 0x07;
        private const byte DS3231_REG_ALARM_2 = 0x0B;
        private const byte DS3231_REG_CONTROL = 0x0E;
        private const byte DS3231_REG_STATUS = 0x0F;
        private const byte DS3231_REG_TEMP = 0x11;
        private const byte DS3231_SRAM_ADDR = 0x08;
        private const byte DS3231_SRAM_SIZE = 56;    

        public DS3231Driver(I2cDevice device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
        }

        public static I2cConnectionSettings GetI2CConnectionSettings()
        {
            var settings = new I2cConnectionSettings(DS3231_ADD)
            {
                BusSpeed = I2cBusSpeed.FastMode
            };

            return settings;
        }

        public DateTime GetTime()
        {
            var rawData = ReadRegion(DS3231_REG_TIME, 7);

            var data = new DS3231Data
            {
                Sec = Bcd2Int(rawData[0]),
                Min = Bcd2Int(rawData[1]),
                Hour = Bcd2Int(rawData[2]),
                Day = Bcd2Int(rawData[3]),
                Date = Bcd2Int(rawData[4]),
                Month = Bcd2Int((byte)(rawData[5] & 0x1F)),
                Century = rawData[5] >> 7
            };

            if (data.Century == 1)
            {
                data.Year = 2000 + Bcd2Int(rawData[6]);
            }
            else
            {
                data.Year = 1900 + Bcd2Int(rawData[6]);
            }

            return new DateTime(data.Year, data.Month, data.Date, data.Hour, data.Min, data.Sec);
        }

        public void SetTime(DateTime time)
        {
            var setData = new byte[7];

            setData[0] = Int2Bcd(time.Second);
            setData[1] = Int2Bcd(time.Minute);
            setData[2] = Int2Bcd(time.Hour);

            setData[3] = Int2Bcd(((int)time.DayOfWeek + 7) % 7);

            setData[4] = Int2Bcd(time.Day);

            if (time.Year >= 2000)
            {
                setData[5] = (byte)(Int2Bcd(time.Month) + 0x80);
                setData[6] = Int2Bcd(time.Year - 2000);
            }
            else
            {
                setData[5] = Int2Bcd(time.Month);
                setData[6] = Int2Bcd(time.Year - 1900);
            }

            WriteRegion(DS3231_REG_TIME, setData);
        }

        /// <summary>
        /// Enable or disable the square wave output.
        /// </summary>
        /// <param name="frequency"></param>
        public void SetSquareWave(DS3231Frequency frequency)
        {
            var controlReg = ReadRegister(DS3231_REG_CONTROL);

            if (frequency >= DS3231Frequency.SQW_OFF)
            {
                controlReg |= 1 << 2;
            }
            else
            {
                controlReg = (byte)((controlReg & 0xE3) | ((byte)frequency << 3));
            }

            WriteRegister(DS3231_REG_CONTROL, controlReg);
        }


        /// <summary>
        ///  Set an alarm time. Sets the alarm registers only.  To cause the
        /// INT pin to be asserted on alarm match, use alarmInterrupt().         *
        /// This method can set either Alarm 1 or Alarm 2, depending on the      *
        /// value of alarmType(use a value from the ALARM_TYPES_t enumeration). *
        /// However, when using this method to set Alarm 1, the seconds value*
        /// is set to zero. (Alarm 2 has no seconds register.)   
        /// </summary>
        /// <param name="alarmType"></param>
        /// <param name="minutes"></param>
        /// <param name="hours"></param>
        /// <param name="dayDate"></param>
        public void SetAlarm(DS3231AlarmType alarmType, int minutes, int hours, int dayDate)
        {
            SetAlarm(alarmType, 0, minutes, hours, dayDate);
        }

        /// <summary>
        /// Set an alarm time. Sets the alarm registers only. To cause the   
        /// INT pin to be asserted on alarm match, use alarmInterrupt().     
        /// This method can set either Alarm 1 or Alarm 2, depending on the   
        /// value of alarmType(use a value from the ALARM_TYPES_t enumeration). 
        /// When setting Alarm 2, the seconds value must be supplied but is    
        /// ignored, recommend using zero. (Alarm 2 has no seconds register.)
        /// </summary>
        /// <param name="alarmType"></param>
        /// <param name="seconds"></param>
        /// <param name="minutes"></param>
        /// <param name="hours"></param>
        /// <param name="day"></param>
        public void SetAlarm(DS3231AlarmType alarmType, int seconds, int minutes, int hours, int day)
        {
            byte address;

            var bcdSeconds = Int2Bcd(seconds);
            var bcdMinutes = Int2Bcd(minutes);
            var bcdHours = Int2Bcd(hours);
            var bcdDay = Int2Bcd(day);

            if (((byte) alarmType & 0x01) != 0) bcdSeconds |= 1 << 7;
            if (((byte) alarmType & 0x02) != 0) bcdMinutes |= 1 << 7;
            if (((byte) alarmType & 0x04) != 0) bcdHours |= 1 << 7;
            if (((byte) alarmType & 0x10) != 0) bcdHours |= 1 << 6;
            if (((byte) alarmType & 0x08) != 0) bcdDay |= 1 << 7;

            if (((byte) alarmType & 0x80) == 0)
            { 
                address = DS3231_REG_ALARM_1;

                WriteRegister(address++, bcdSeconds);
            }
            else
            {
                address = DS3231_REG_ALARM_2;
            }

            WriteRegister(address++, bcdMinutes);

            WriteRegister(address++, bcdHours);

            WriteRegister(address, bcdDay);
        }

        /// <summary>
        /// Enable or disable an alarm "interrupt" which asserts the INT pin
        /// </summary>
        /// <param name="alarmNumber"></param>
        /// <param name="interruptEnabled"></param>
        public void SetAlarmInterrupt(ushort alarmNumber, bool interruptEnabled)
        {
            var controlReg = ReadRegister(DS3231_REG_CONTROL);

            var mask = (byte)(0 << (alarmNumber - 1));

            if (interruptEnabled)
            {
                controlReg |= mask;
            }
            else
            {
                controlReg &= (byte) ~mask;
            }

            WriteRegister(DS3231_REG_CONTROL, controlReg);
        }

        /// <summary>
        /// Returns true or false depending on whether the given alarm has been 
        /// triggered, and resets the alarm flag bit.
        /// </summary>
        /// <param name="alarmNumber"></param>
        /// <returns></returns>
        public bool GetAlarmStatus(int alarmNumber)
        {
            var statusReg = ReadRegister(DS3231_REG_STATUS);

            var mask = (byte) (1 << 0 << (alarmNumber - 1));

            if ((statusReg & mask) != 0)
            {
                statusReg &= (byte)~mask;

                WriteRegister(DS3231_REG_STATUS, statusReg);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks the OSF bit in the status register which indicates that the 
        /// oscillator is or was stopped.
        /// </summary>
        /// <returns></returns>
        public bool GetOscillatorStatus()
        {
            return (ReadRegister(DS3231_REG_STATUS) &  1 << 7) != 0;
        }

        public byte[] GetRam()
        {
            return ReadRegion(DS3231_SRAM_ADDR, DS3231_SRAM_SIZE);
        }

        public void SetRam(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (data.Length != DS3231_SRAM_SIZE)
                throw new ArgumentOutOfRangeException(nameof(data), "Invalid length");

            WriteRegion(DS3231_SRAM_ADDR, data);
        }

        /// <summary>
        /// Returns the temperature in Celsius.  
        /// </summary>
        /// <returns></returns>
        public double GetTemp()
        {
            var data = ReadRegion(DS3231_REG_TEMP, 2);

            return data[0] + (data[1] >> 6) * 0.25;
        }

        public void SetSystemTime()
        {
            SystemTime.SetTime(GetTime());
        }

        private static int Bcd2Int(byte bcd)
        {
            return bcd / 16 * 10 + (bcd % 16);
        }

        private static byte Int2Bcd(int dec)
        {
            return (byte)(dec / 10 * 16 + dec % 10);
        }

        private byte ReadRegister(byte address)
        {
            return ReadRegion(address, 1)[0];
        }

        private byte[] ReadRegion(byte address, int length)
        {
            var txBuffer = new[] { address };

            var rxBuffer = new byte[length];

            _device.WriteRead(txBuffer, rxBuffer);

            return rxBuffer;
        }

        private void WriteRegister(byte address, byte value)
        {
            WriteRegion(address, new byte[] { value });
        }

        private void WriteRegion(byte address, byte[] data)
        {
            var txBuffer = new byte[data.Length + 1];

            txBuffer[0] = address;

            data.CopyTo(txBuffer, 1);

            _device.Write(txBuffer);
        }
    }
}
