using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectOutput.Cab.Out.AdressableLedStrip
{
    /// <summary>
    /// The WemosD1MPStripController is a Teensy equivalent board called Wemos D1 Mini Pro, it's cheaper than the Teensy and can support the same amount of Ledstrip & leds per strip.
    /// 
    /// \image html wemos-d1-mini-pro.jpg
    /// 
    /// The firmware for the WemosD1 can be found there <a target="_blank" href="http://github.com/yoyofr/PincabLedStrip/">yoyofr Github page (latest version using this DirectOutput controller)</a> and there <a target="_blank" href="https://github.com/aetios50/PincabLedStrip">aetios50 Github page (firmware reference page)</a>
    /// 
    /// You can also find a tutorial (in french for now) explaining the flashing process for the Wemos D1 Mini Pro using these firmware <a target="_blank" href="https://shop.arnoz.com/laboratoire/2019/10/29/flasher-unewemos-d1-mini-pro-pour-lutiliser-dans-son-pincab/">Arnoz' lab Wemos D1 flashing tutorial</a>
    /// 
    /// There is also a great online tool to setup easily both Teensy & Wemos D1 based ledstrips <a target="_blank" href="https://shop.arnoz.com/laboratoire/2020/09/11/cacabinet-generator-for-english-speaking-dude/">Arnoz' cacabinet generator</a>
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
        /// <remarks>
        /// These commands are supported by the new Wemos firmware from yoyofr & aetios50, supporting dynamic ledstrip length setup (part of an overall performance improvement)
        /// </remarks>
        public bool SendPerLedstripLength
        {
            get { return _SendPerLedstripLength; }
            set {
                _SendPerLedstripLength = value;
            }
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
        }
    }
}
