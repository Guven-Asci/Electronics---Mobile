using System;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

//using ServiceStack.Text;

namespace SharpCircuit {
	
	class Program {

        public static void LogSharp(params object[] objs)
        {
            StringBuilder sb = new StringBuilder();
            foreach (object o in objs)
                sb.Append(o.ToString()).Append(" ");
            Debug.Log(sb.ToString());
        }

        public static void LogF(string format, params object[] objs)
        {
            Debug.Log(string.Format(format, objs));
        }

		public static double Round(double val, int places) {
			if(places < 0) throw new ArgumentException("places");
			return Math.Round(val - (0.5 / Math.Pow(10, places)), places);
		}

		static void Main(string[] args) {

			Circuit sim = new Circuit();

			var volt0 = sim.Create<VoltageInput>(Voltage.WaveType.DC);
            volt0.maxVoltage = 10;
			var res0 = sim.Create<Resistor>(6);
            var res1 = sim.Create<Resistor>(2);
            var res2 = sim.Create<Resistor>(6);
            var res3 = sim.Create<Resistor>(2);
            var res4 = sim.Create<Resistor>(6);
            var res5 = sim.Create<Resistor>(1000);
			var ground0 = sim.Create<Ground>();

			sim.Connect(volt0.leadPos, res0.leadIn);
            sim.Connect(res0.leadOut, res1.leadIn);
            sim.Connect(volt0.leadPos, res2.leadIn);
            sim.Connect(res2.leadOut, res3.leadIn);
            sim.Connect(res3.leadOut, ground0.leadIn);
			sim.Connect(res1.leadOut, ground0.leadIn);
            sim.Connect(res3.leadOut, res4.leadIn);
            sim.Connect(res4.leadOut, res1.leadIn);
            
			for(int x = 1; x <= 1; x++) {
				sim.doTick();
				// Ohm's Law
				LogSharp(res0.getVoltageDelta(), res0.resistance * res0.getCurrent()); // V = I x R
				LogSharp(res0.getCurrent(), res0.getVoltageDelta() / res0.resistance); // I = V / R
				LogSharp(res0.resistance, res0.getVoltageDelta() / res0.getCurrent()); // R = V / I
                
                LogSharp(volt0.getCurrent());
			}

			Debug.Log("program complete");
			
		}

	}
}


    

