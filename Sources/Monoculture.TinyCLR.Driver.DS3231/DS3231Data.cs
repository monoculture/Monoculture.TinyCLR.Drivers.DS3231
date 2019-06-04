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
    internal class DS3231Data
    {
        public int Sec { get; set; }
        public int Min { get; set; }
        public int Hour { get; set; }
        public int Day { get; set; }
        public int Date { get; set; }
        public int Month { get; set; }
        public int Century { get; set; }
        public int Year { get; set; }
    }
}
