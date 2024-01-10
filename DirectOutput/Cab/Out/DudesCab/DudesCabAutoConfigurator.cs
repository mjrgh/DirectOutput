﻿using DirectOutput.Cab.Toys.LWEquivalent;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;

namespace DirectOutput.Cab.Out.DudesCab
{
	public class DudesCabAutoConfigurator : IAutoConfigOutputController
	{
		#region IAutoConfigOutputController Member

		/// <summary>
		/// This method detects and configures Dude's Cab from l'atelier d'Arnoz (arnoz.com) output controllers automatically.
		/// </summary>
		/// <param name="Cabinet">The cabinet object to which the automatically detected IOutputController objects are added if necessary.</param>
		public void AutoConfig(Cabinet Cabinet)
		{
			const int UnitBias = 89;
			List<string> Preconfigured = new List<string>(Cabinet.OutputControllers.Where(OC => OC is DudesCab).Select(PO => ((DudesCab)PO).ComPort));
			String comPort = GetDevice();

			if (!Preconfigured.Contains(comPort) && comPort != "")
			{
				DudesCab p = new DudesCab(comPort);
				if (!Cabinet.OutputControllers.Contains(p.Name))
				{
					Cabinet.OutputControllers.Add(p);
					Log.Write("Detected and added DudesCab Controller Nr. {0} with name {1}".Build(p.Number, p.Name));

					if (!Cabinet.Toys.Any(T => T is LedWizEquivalent && ((LedWizEquivalent)T).LedWizNumber == p.Number + UnitBias))
					{
						LedWizEquivalent LWE = new LedWizEquivalent();
						LWE.LedWizNumber = p.Number + UnitBias;
						LWE.Name = "{0} Equivalent".Build(p.Name);

						for (int i = 1; i <= p.NumberOfOutputs; i++)
						{
							LedWizEquivalentOutput LWEO = new LedWizEquivalentOutput() { OutputName = "{0}\\{0}.{1:00}".Build(p.Name, i), LedWizEquivalentOutputNumber = i };
							LWE.Outputs.Add(LWEO);
						}

						if (!Cabinet.Toys.Contains(LWE.Name))
						{
							Cabinet.Toys.Add(LWE);
							Log.Write("Added LedwizEquivalent Nr. {0} with name {1} for DudesCab Controller Nr. {2}".Build(
								LWE.LedWizNumber, LWE.Name, p.Number) + ", {0}".Build(p.NumberOfOutputs));
						}
					}
				}
			}

		}

		public static String GetDevice()
		{
			foreach (string sp in System.IO.Ports.SerialPort.GetPortNames())
			{
				SerialPort Port = null;
				try
				{
					Port = new SerialPort(sp, 115200, Parity.None, 8, StopBits.One);
					Port.NewLine = "\r\n";
					Port.ReadTimeout = 100;
					Port.WriteTimeout = 100;
					Port.Open();
					Port.DtrEnable = true;
					Port.Write(new byte[] { 0, 251, 0, 0, 0, 0, 0, 0, 0 }, 0, 9);
					while (true)
					{
						string result = Port.ReadLine();
						if (result == "Beertime, DudesCab is Connected")
						{
							Port.Close();
							return sp;
						}
					}
				}
				catch (Exception ex)
				{
					if (Port != null)
					{
						Port.Close();
					}
				}
			}
			return "";
		}

		#endregion
	}
}
