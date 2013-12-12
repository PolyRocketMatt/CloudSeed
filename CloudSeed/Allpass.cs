﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudSeed
{
	public class Allpass
	{
		public double Samplerate;
		public double Feedback;
		public int DelaySamples;

		public double ModAmount;
		public double HiCutAmount;

		double _modFreq;
		public double ModFreq
		{
			get { return _modFreq; }
			set
			{
				_modFreq = value;
				ModIncrement = 1.0 / Samplerate * _modFreq;
			}
		}

		double _hiCut;
		public double HiCut
		{
			get { return _hiCut; }
			set
			{
				_hiCut = value;
				Alpha = Math.Exp(-2 * Math.PI * _hiCut / Samplerate);
			}
		}

		private double ModPhase;
		private double ModValue;
		private double ModIncrement;

		private double Alpha;
		private double[] Buffer;
		private double[] BufferOut;
		private int I;

		private double A;
		private double AOut;

		public Allpass()
		{
			Buffer = new double[48000];
			Feedback = 0.7;
		}

		public void UpdateMod(int sampleCount)
		{
			ModPhase += sampleCount * ModIncrement;
			if (ModPhase > 1.0)
				ModPhase -= 1.0;

			ModValue = Math.Sin(ModPhase * 2 * Math.PI);
		}

		public double Process(double x)
		{
			var len = Buffer.Length;
			var k = (int)(I + DelaySamples * (1 + 0.01 * ModAmount * ModValue)) % len;

			var bufOut = Buffer[k];
			var bufIn = x - Feedback * bufOut;
			Buffer[I] = bufIn;
			var y = Feedback * bufIn + bufOut;

			I--;
			if (I < 0)
				I += len;

			A = (1 - Alpha) * y + Alpha * A;
			AOut = A * HiCutAmount + y * (1 - HiCutAmount);
			return AOut;
		}
	}
}
