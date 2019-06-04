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
using System.Diagnostics;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.Devices.I2c;

namespace Monoculture.TinyCLR.Drivers.DS3231.Demo
{
    public class MainX
    {
        public static void Main()
        {
            //GetTemp();

            GetTime();
            SetTime();

            //SetSystemTime();
            //GetOscillatorStatus();

        }

        private static void GetTemp()
        {
            var driver = GetDriver();

            var temp = driver.GetTemp();

            Debug.WriteLine(temp.ToString());
        }

        private static void SetSystemTime()
        {
            var driver = GetDriver();

            driver.SetSystemTime();
        }

        private static void GetTime()
        {
            var driver = GetDriver();

            var dateTime = driver.GetTime();

            Debug.WriteLine(dateTime.ToString());
        }

        public static void SetTime()
        {
            var driver = GetDriver();

            var dateTime = new DateTime(1994, 1,1,1,1,1);

            driver.SetTime(dateTime);
        }

        private static void GetOscillatorStatus()
        {
            var driver = GetDriver();

            var stopped = driver.GetOscillatorStatus();

            Debug.WriteLine(stopped.ToString());
        }

        private static DS3231Driver GetDriver()
        {
            var settings = DS3231Driver.GetI2CConnectionSettings();

            var controller = I2cController.FromName(G120E.I2cBus.I2c0);

            var device = controller.GetDevice(settings);

            var driver = new DS3231Driver(device);

            return driver;
        }
    }
}
