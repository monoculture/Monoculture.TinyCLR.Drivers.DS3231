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

namespace Monoculture.TinyCLR.Drivers.DS3231
{
    public enum DS3231AlarmType
    {
        Alm1EverySecond = 0x0F,
        Alm1MatchSeconds = 0x0E,
        Alm1MatchMinutes = 0x0C,     //match minutes *and* seconds
        Alm1MatchHours = 0x08,       //match hours *and* minutes, seconds
        Alm1MatchDate = 0x00,        //match date *and* hours, minutes, seconds
        Alm1MatchDay = 0x10,         //match day *and* hours, minutes, seconds
        Alm2EveryMinute = 0x8E,
        Alm2MatchMinutes = 0x8C,     //match minutes
        Alm2MatchHours = 0x88,       //match hours *and* minutes
        Alm2MatchDate = 0x80,        //match date *and* hours, minutes
        Alm2MatchDay = 0x90
    }
}
