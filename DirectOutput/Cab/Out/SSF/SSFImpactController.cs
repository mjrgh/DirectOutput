using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ManagedBass;
using ManagedBass.Fx; 
using ManagedBass.Mix;

// <summary>
// This namespace contains a outputcontroller implementaion which isnt doing anything.
// </summary>
namespace DirectOutput.Cab.Out.SSFImpactController
{

    /// <summary>
    /// The SSFImpactor supports for using common audio bass shakers to emulate/simulate contractors/solenoids in a Virtual Pinball Cabinet.<br/>
    /// It presents itself as a full set of contactors, etc for assignment via DOF Config. Support, hardware suggestions, layout diagrams and the
    /// just about friendliest people in the hobby can be found here:  <a href="https://www.facebook.com/groups/SSFeedback/"/>
    /// <remarks>For help specifically with SSFImpactor look fo Kai "MrKai" Cherry.</remarks>
    /// </summary>
    public class SSFImpactController : OutputControllerBase, IOutputController
    {
        internal string _Speakers = "Rear";
        internal string _Shaker1 = "RearCenter";
        internal string _Shaker2 = "Rear";
        private bool _LowImpactMode = false;
        internal int _DeviceNumber = -1;
        internal float _ImpactAmount = 1F;
        internal float _ShakeAmount = 1F;
        internal uint _TargetChannels = 0;  // Uint because BassFlags enum causes problem here?
        internal uint _ShakerChannel1 = 0;
        internal uint _ShakerChannel2 = 0;
        internal float _FlipperVolume = 0.50F;
        internal float _BumperVolume = 0.75F;
        internal float _SlingsEtAlVolume = 1.0F;

        /// <summary>
        /// Gets or sets speakers that SSF will send impactor samples to
        /// </summary>
        /// <value>
        /// One of the ManagedBass.Speaker* enums, i.e. 
        /// </value>
        public string Speakers
        {
            get { return _Speakers; }
            set { _Speakers = value; }
        }
        public string BassShaker1
        {
            get { return _Shaker1; }
            set { _Shaker1 = value; }
        }
        public string BassShaker2
        {
            get { return _Shaker2; }
            set { _Shaker2 = value; }
        }
        public string LowImpactMode
        {
            get { return _LowImpactMode.ToString(); }
            set { bool.TryParse(value, out _LowImpactMode); }
        }

        public int DeviceNumber
        {
            get { return _DeviceNumber; }
            set { _DeviceNumber = value; }
        }

        public float ImpactFactor
        {
            get { return _ImpactAmount; }
            set { _ImpactAmount = value; }
        }
        public float ShakerImpactFactor
        {
            get { return _ShakeAmount; }
            set { _ShakeAmount = value; }
        }
        public float FlipperLevel
        {
            get { return _FlipperVolume; }
            set { _FlipperVolume = value; }
        }
        public float BumperLevel
        {
            get { return _BumperVolume; }
            set { _BumperVolume = value; }
        }
        public float SlingsLevel
        {
            get { return _SlingsEtAlVolume; }
            set { _SlingsEtAlVolume = value; }
        }


        internal SoundBank bank = new SoundBank();
        internal List<String> myNames = new List<String>();
        internal List<SSFnoid> Contactors = new List<SSFnoid>();

        internal Assembly assembly = Assembly.GetExecutingAssembly();
        internal Stream SSF = Assembly.GetExecutingAssembly().GetManifestResourceStream("DirectOutput.Cab.Out.SSF.SSF1C");
        internal Stream SSFLI = Assembly.GetExecutingAssembly().GetManifestResourceStream("DirectOutput.Cab.Out.SSF.SSFLI"); //low intensity
        internal MemoryStream ssfStream = new MemoryStream();
        internal bool haveBass, useFaker = false;
        internal Faker fakeShaker;
        internal int stream = 0, stream1 = 0;

        /// <summary>
        /// Init initializes the ouput controller.<br />
        /// This method is called after the
        /// objects haven been instanciated.
        /// 
        /// Specifically, Init is prepping the "Soundbank" for the currently supported 'hardware'
        /// and setting the user preffered output style/profile via presence, or lack of, 
        /// a file named "SSFLI" (Surround Sound Feedback - Low Intensity)
        /// </summary>
        /// <param name="Cabinet">The cabinet object which is using the output controller instance.</param>
        public override void Init(Cabinet Cabinet)
        {
            if (bank == null)
            {
                Log.Exception("Could not Initialize SSFImpactor");
                return;
            }

            try
            {
                _TargetChannels = (uint)(BassFlags)Enum.Parse(typeof(BassFlags), "Speaker" + _Speakers);
                _ShakerChannel1 = (uint)(BassFlags)Enum.Parse(typeof(BassFlags), "Speaker" + _Shaker1);
                _ShakerChannel2 = (uint)(BassFlags)Enum.Parse(typeof(BassFlags), "Speaker" + _Shaker2);

            }
            catch
            {
                Log.Write("Invalid value for Speakers in Cabinet.xml: " + _Speakers);
                _TargetChannels = (uint)BassFlags.SpeakerRear;
                _ShakerChannel1 = (uint)BassFlags.SpeakerRearCenter;
                _ShakerChannel2 = (uint)BassFlags.SpeakerRear;
            }


            if (SoundBank.Names.Count == 0)
            {
                try
                {
                    bank.PrepBox(_DeviceNumber);
                    var info = Bass.Info;
                    try
                    {
                        for (int dev = 1; ; dev++)
                        {
                            var bd = Bass.GetDeviceInfo(dev);
                            Log.Write("BASS device " + dev.ToString() + " is " + bd.Name);
                        }
                    }
                    catch (Exception)
                    {
                        // You have to wait until GetDeviceInfo fails.  Yuck.
                    }

                    Log.Write("BASS detects " + info.SpeakerCount.ToString() + " speakers.");

                    if (_LowImpactMode || File.Exists(@"C:\DirectOutput\SSFLI"))
                    {
                        SSFLI.CopyTo(ssfStream);
                        SSF = null;
                    }
                    else
                    {
                        SSF.CopyTo(ssfStream);
                        SSFLI = null;
                    }

                    stream = Bass.CreateStream(ssfStream.ToArray(), 0, ssfStream.Length, (BassFlags)_TargetChannels);
                    stream1 = Bass.CreateStream(ssfStream.ToArray(), 0, ssfStream.Length, BassFlags.SpeakerRearCenter);

                    useFaker = true;
                    fakeShaker = new Faker();
                    fakeShaker.Shaker1 = _ShakerChannel1;
                    fakeShaker.Shaker2 = _ShakerChannel2;
                    fakeShaker.ImpactEffect = _ShakeAmount;

                    Log.Write("SSFShaker activated");
                    Log.Write("SSFImpactor \"Hardware\" Initialized\n");
                    haveBass = true;
                    AddOutputs();
                }
                catch (Exception e)
                {
                    Log.Write("Could Not Initialze Bass - " + e.Message);
                }
            }



        }

        /// <summary>
        /// Finishes the ouput controller.<br/>
        /// All necessary cleanup tasks have to be implemented here und all physical outputs have to be turned off.
        /// </summary>
        public override void Finish()
        {
            if (!haveBass)
            {
                Log.Write("!haveBass test kickout, Finish()");
                return;
            }

            if (stream != 0)
            {
                Bass.StreamFree(stream);
            }

            Outputs = null;
            try
            {
                Bass.Free();
            }
            catch (Exception)
            {
                Log.Write("Bass subsystem was not initialized");
            }

        }

        /// <summary>
        /// Update must update the physical outputs to the values defined in the Outputs list. 
        /// </summary>
        public override void Update()
        {

            try
            {
                foreach (IOutput outp in Outputs)
                {

                    if (outp.Number == 11)
                    {
                        fakeShaker.SetSpeed(outp.Value);
                        continue;
                    }



                    if (Contactors[outp.Number].fired && (Contactors[outp.Number].Value == outp.Value))
                    {

                        continue;
                    }

                    if (outp.Value != 0)
                    {
                       
                        if (stream != 0)
                        {
                            if (outp.Number < 4 || outp.Number > 9)
                            {
                                Bass.ChannelSetAttribute(stream, ChannelAttribute.Volume, _SlingsEtAlVolume); //Per Rusty, do "front 4" and extras 'harder'
                                Bass.ChannelSetAttribute(stream1, ChannelAttribute.Volume, _SlingsEtAlVolume * _ImpactAmount);
                            }
                            else
                            {
                                Bass.ChannelSetAttribute(stream, ChannelAttribute.Volume, _BumperVolume); //pop bumpers, etc are further away, generally :)
                                Bass.ChannelSetAttribute(stream1, ChannelAttribute.Volume, _BumperVolume * _ImpactAmount);
                            }


                            if (outp.Number < 2) //the flippers
                            {
                                Bass.ChannelSetAttribute(stream, ChannelAttribute.Volume, _FlipperVolume); //HOWEVER...flips don't need 'Full Hollywood' maybe :)
                                Bass.ChannelSetAttribute(stream, ChannelAttribute.Volume, _FlipperVolume * _ImpactAmount);
                            }

                            //"mixing" - .Mix seems to just not work "inside" vpx, so...
                          
                            //Log.Write("Firing " + outp.Name);
                            Bass.ChannelPlay(stream);
                            if (_LowImpactMode == false) //lay off in LI mode
                            {
                                Bass.ChannelPlay(stream1);
                            }
                            
                            Contactors[outp.Number].fired = true;
                            Contactors[outp.Number].Value = outp.Value;
                            
                        }
                    }
                    else
                    {
                        Contactors[outp.Number].fired = false;
                        Contactors[outp.Number].Value = 0;
                    }
                }


            }
            catch (Exception e)
            {

                Log.Write("UPDATE EXCEPTION:: " + e.Message);
            }

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SSFImpactController"/> class.
        /// </summary>
        public SSFImpactController()
        {
            Outputs = new OutputList();


        }


        /// <summary>
        /// This method is called whenever the value of a output in the Outputs property changes its value.<br />
        /// Due to some clever "orthogonal thinking", the whole of "hardware suite" can be controled here
        /// Hattip to djrobx for the idea!
        /// </summary>
        /// <param name="Output">The output.</param>
        protected override void OnOutputValueChanged(IOutput Output)
        {
            if (Output.Number > Contactors.Count - 1)
            {
                // Log.Write(String.Format("BYPASS:: Ouput.Number ->{0} Outputs[{0}].Value ->{1}, current val not changed, non zero", Output.Number, Output.Value));
                return;
            }


            Outputs[Output.Number].Value = Output.Value;
        }
        /// <summary>
        /// Adds the outputs from the SoundBank.<br/>
        /// This method adds OutputNumbered objects for all outputs to the list of outputs.
        /// </summary>
        internal void AddOutputs()
        {

            for (int i = 0; i <= SoundBank.Names.Count - 1; i++)
            {
                if (!Outputs.Any(x => x.Number == i))
                {
                    Outputs.Add(new Output() { Name = "{0}.{1:00}".Build(SoundBank.Names[i], i), Number = i, Value = 0 });
                    Contactors.Add(new SSFnoid() { Number = i, Value = 0 }); //yes, .11 is Shaker...
                    myNames.Add(SoundBank.Names[i]);
                    Log.Write($"Added: {Outputs.Last().Name} to internal list...");

                }
            }
        }

    }

    /// <summary>
    /// The SSFNoid is a simple class for storing state information on the virtual contactors.<br/>
    /// It can be 
    /// </summary>
    class SSFnoid
    {
        internal bool fired = false;
        public byte Value = 0;
        public int Number;

        static SSFnoid()
        {

        }

        public void Activate()
        {

        }
    }


    class Faker
    {
        internal bool isShaking = false;
        public byte currentValue = 0;
        internal int running, running2;
        internal uint _ShakerChannel1 = 0;
        internal uint _ShakerChannel2 = 0;
        internal float _impactMod = 1.0F;

        internal Stream PE = Assembly.GetExecutingAssembly().GetManifestResourceStream("DirectOutput.Cab.Out.SSF.7hzOD"); //PE40Hz1s
        internal Stream PE1 = Assembly.GetExecutingAssembly().GetManifestResourceStream("DirectOutput.Cab.Out.SSF.7hzOD");
        internal MemoryStream runstream = new MemoryStream();
        internal MemoryStream runstream2 = new MemoryStream();
        

        public uint Shaker1
        {
            set {
                Log.Write("Shaker1 set to:" + (BassFlags)value);
                _ShakerChannel1 = value;
            }
        }
        public uint Shaker2
        {
            set { _ShakerChannel2 = value; }
        }
        public float ImpactEffect
        {
            set { _impactMod = value; }
        }

        static Faker()
        {
            Log.Write("Using SSFImpactor Shaking");


        }

        public void TurnOn()
        {
            if (isShaking)
            {
                return;
            }

            //S1W.CopyTo(startstream);
            PE.CopyTo(runstream);
            PE1.CopyTo(runstream2);

            Log.Write("Shaker::ON");
            isShaking = true;
        }

        public void TurnOff()
        {

            if (isShaking && currentValue == 0)
            {
                Bass.ChannelStop(running);
                Bass.ChannelStop(running2);
                isShaking = false;

                Log.Write("Shaker::OFF");

            }
        }


        public void SetSpeed(byte speed)
        {
            if (speed == 0)
            {
                TurnOff();
                currentValue = 0;

                return;
            }

            if (speed == currentValue) // reality: speed == currentValue
            {
                return;
            }

            if (!isShaking)
            {
                TurnOn();
                currentValue = speed;
            }

            if (running == 0)
            {
                running = Bass.CreateStream(runstream.ToArray(), 0, runstream.Length, (BassFlags)_ShakerChannel1); //perfect loop sample
                Log.Write("running set to speaker::" + (BassFlags)_ShakerChannel1);
                running2 = Bass.CreateStream(runstream2.ToArray(), 0, runstream.Length, (BassFlags)_ShakerChannel2);
                Bass.ChannelAddFlag(running, BassFlags.Loop);
                Bass.ChannelAddFlag(running2, BassFlags.Loop);
            }

            float myFloat = speed;
            Log.Write("FloatedSpeed::" + myFloat.ToString());
            Bass.ChannelSetAttribute(running, ChannelAttribute.Volume, (myFloat / 255) * _impactMod); //for variability:  (speed/255)
            Bass.ChannelSetAttribute(running2, ChannelAttribute.Volume, (myFloat / 255) * _impactMod); // * _impactMod
            Bass.ChannelPlay(running);
            Bass.ChannelPlay(running2);

        }

    }

    class SoundBank
    {
        private static List<int> ports = new List<int>();
        private static List<String> names = new List<String>();
        public static List<String> Names { get => names; set => names = value; }
        public static List<int> Ports { get => ports; set => ports = value; }

        //This is a separate class to allow for future expansion; eg parameters to change playback characteristics
        //It is a remnant of the 'soundbank'-based experiments, but future forward can serve a similar purpose via modifiers, etc
        //
        static SoundBank()
        {

        }

        public List<String> BankNames()
        {
            return names;
        }

        public void PrepBox(int DeviceNumber)
        {
            try
            {
                Bass.Init(DeviceNumber);
            }
            catch (Exception)
            {
                Log.Write("Could Not Initialze Bass. SSFImpactor disabled for events.");
                return;
            }

            ports = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };
            names = new List<String> {  "FlipperLeft", "FlipperRight", "SlingshotLeft", "SlingshotRight",
                "10-BumperBackLeft", "10-BumperBackCenter","10-BumperBackRight",
                "10-BumperMiddleLeft", "10-BumperMiddleCenter", "10-BumperMiddleRight", "Knocker","Shaker","Gear",
                "HellBallMotor","Bell" }; ;

            int max = ports.Count - 1;

            Log.Write("Initializing SSFImpactor 'Hardware'...\n");

            for (int i = 0; i < max; i++)
            {
                Log.Write(String.Format("PORT {0}: {1}", i.ToString(), names[i]));
            }

            Log.Write(String.Format("SSFImpactor: {0} ready.", ports.Count.ToString()));
        }

    }

}