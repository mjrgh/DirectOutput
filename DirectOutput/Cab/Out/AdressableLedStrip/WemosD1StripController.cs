using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectOutput.Cab.Out.AdressableLedStrip
{
    /// <summary>
    /// The WemosD1MPStripController is a Teensy equivalent board called Wemos D1 Mini Pro (also known as ESP8266), it's cheaper than the Teensy and can support the same amount of Ledstrip & leds per strip.
    /// 
    /// \image html wemos-d1-mini-pro.jpg
    /// 
    /// The Wemos D1 Mini Pro firmware made by aetios50, peskopat & yoyofr can be found there <a target="_blank" href="https://github.com/aetios50/PincabLedStrip">aetios50 Github page</a>
    /// 
    /// You can also find a tutorial (in french for now) explaining the flashing process for the Wemos D1 Mini Pro using these firmware <a target="_blank" href="https://shop.arnoz.com/laboratoire/2019/10/29/flasher-unewemos-d1-mini-pro-pour-lutiliser-dans-son-pincab/">Arnoz' lab Wemos D1 flashing tutorial</a>
    /// 
    /// There is a great online tool to setup easily both Teensy & Wemos D1 based ledstrips (an english version is also available) <a target="_blank" href="https://shop.arnoz.com/laboratoire/2020/09/17/cacabinet-generator/">Arnoz' cacabinet generator</a>
    /// 
    /// </summary>
    public class WemosD1MPStripController : TeensyStripController
    {
        private bool _SendPerLedstripLength = false;

        /// <summary>
        /// Set if the controller will send per ledstrip length commands during the handshake.
        /// </summary>
        /// <value>
        /// true if the commands are sent
        /// </value>
        public bool SendPerLedstripLength
        {
            get { return _SendPerLedstripLength; }
            set {
                _SendPerLedstripLength = value;
            }
        }

        private string _PerLedstripBrightness = string.Empty;

        /// <summary>
        /// Set the brightness per ledstrip in the range 0->255
        /// </summary>
        /// <value>
        /// a string containing brightness values separated by spaces
        /// </value>
        public string PerLedstripBrightness
        {
            get { return _PerLedstripBrightness; }
            set { _PerLedstripBrightness = value; }
        }

        protected override void SetupController()
        {
            byte[] ReceiveData = null;
            int BytesRead = -1;
            byte[] CommandData = null;

            base.SetupController();

            //Send number of leds per leds strips 
            if (SendPerLedstripLength) {
                for (var numled = 0; numled < NumberOfLedsPerStrip.Length; ++numled) {
                    int nbleds = NumberOfLedsPerStrip[numled];
                    if (nbleds > 0) {
                        CommandData = new byte[5] { (byte)'Z', (byte)numled, (byte)(NumberOfLedsPerStrip.Length - 1), (byte)(nbleds >> 8), (byte)(nbleds & 255) };
                        Log.Debug($"Resize ledstrip {numled} to {nbleds} leds.");
                        ComPort.Write(CommandData, 0, 5);
                        ReceiveData = new byte[1];
                        BytesRead = -1;
                        try {
                            BytesRead = ReadPortWait(ReceiveData, 0, 1);
                        } catch (Exception E) {
                            throw new Exception($"Expected 1 bytes after setting the number of leds for ledstrip {numled} , but the read operation resulted in a exception. Will not send data to the controller.", E);
                        }

                        if (BytesRead != 1 || ReceiveData[0] != (byte)'A') {
                            throw new Exception($"Expected a Ack (A) after setting the number of leds for ledstrip {numled}, but received no answer or a unexpected answer ({(char)ReceiveData[0]}). Will not send data to the controller.");
                        }
                    }
                }
            }

            //Send brightness per ledstrip if available
            if (!PerLedstripBrightness.IsNullOrEmpty()) {
                var values = PerLedstripBrightness.Split(' ');
                var minlen = Math.Min(NumberOfLedsPerStrip.Length, values.Length);
                for (var numled = 0; numled < minlen; ++numled) {
                    var brightness = 0;
                    try {
                        brightness = Int32.Parse(values[numled]).Limit(0, 255);
                    } catch (Exception E) {
                        throw new Exception($"Cannot parse brigthness value for ledstrip {numled}, check if there are only [0-255] ranged numbers separated by spaces.", E);
                    }
                    CommandData = new byte[4] { (byte)'B', (byte)numled, (byte)(minlen - 1), (byte)(brightness) };
                    Log.Debug($"Send brightness {brightness} for ledstrip {numled} [{string.Join(" ", CommandData)}].");
                    ComPort.Write(CommandData, 0, 4);
                    ReceiveData = new byte[1];
                    BytesRead = -1;
                    try {
                        BytesRead = ReadPortWait(ReceiveData, 0, 1);
                    } catch (Exception E) {
                        throw new Exception($"Expected 1 bytes after setting the brightness for ledstrip {numled} , but the read operation resulted in a exception. Will not send data to the controller.", E);
                    }

                    if (BytesRead != 1 || ReceiveData[0] != (byte)'A') {
                        throw new Exception($"Expected a Ack (A) after setting the brightness for ledstrip {numled}, but received no answer or a unexpected answer ({(char)ReceiveData[0]}). Will not send data to the controller.");
                    }
                }
            }
        }
    }
}
