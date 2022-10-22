using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Broccoli.Examples
{
	/// <summary>
	/// Easing functions.
	/// </summary>
	public class Easing {
		/// <summary>
		/// Linear the specified start, end and value.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float Linear(float start, float end, float value){
			return Mathf.Lerp(start, end, value);
		}
		/// <summary>
		/// Clerp easing function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float Clerp(float start, float end, float value){
			float min = 0.0f;
			float max = 360.0f;
			float half = Mathf.Abs((max - min) * 0.5f);
			float retval = 0.0f;
			float diff = 0.0f;
			if ((end - start) < -half){
				diff = ((max - start) + end) * value;
				retval = start + diff;
			}else if ((end - start) > half){
				diff = -((max - end) + start) * value;
				retval = start + diff;
			}else retval = start + (end - start) * value;
			return retval;
		}
		/// <summary>
		/// Spring easing funtion.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float Spring(float start, float end, float value){
			value = Mathf.Clamp01(value);
			value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
			return start + (end - start) * value;
		}
		/// <summary>
		/// EaseInQuad function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseInQuad(float start, float end, float value){
			end -= start;
			return end * value * value + start;
		}
		/// <summary>
		/// Eases the out quad.
		/// </summary>
		/// <returns>The out quad.</returns>
		/// <param name="start">Start.</param>
		/// <param name="end">End.</param>
		/// <param name="value">Value.</param>
		public static float EaseOutQuad(float start, float end, float value){
			end -= start;
			return -end * value * (value - 2) + start;
		}
		/// <summary>
		/// EaseInOutQuad function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseInOutQuad(float start, float end, float value){
			value /= .5f;
			end -= start;
			if (value < 1) return end * 0.5f * value * value + start;
			value--;
			return -end * 0.5f * (value * (value - 2) - 1) + start;
		}
		/// <summary>
		/// EaseInCubic function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseInCubic(float start, float end, float value){
			end -= start;
			return end * value * value * value + start;
		}
		/// <summary>
		/// EaseOutCubic function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseOutCubic(float start, float end, float value){
			value--;
			end -= start;
			return end * (value * value * value + 1) + start;
		}
		/// <summary>
		/// EaseInOutCubic function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseInOutCubic(float start, float end, float value){
			value /= .5f;
			end -= start;
			if (value < 1) return end * 0.5f * value * value * value + start;
			value -= 2;
			return end * 0.5f * (value * value * value + 2) + start;
		}
		/// <summary>
		/// EaseInQuart function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseInQuart(float start, float end, float value){
			end -= start;
			return end * value * value * value * value + start;
		}
		/// <summary>
		/// EaseOutQuart function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseOutQuart(float start, float end, float value){
			value--;
			end -= start;
			return -end * (value * value * value * value - 1) + start;
		}
		/// <summary>
		/// EaseInOutQuart function
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseInOutQuart(float start, float end, float value){
			value /= .5f;
			end -= start;
			if (value < 1) return end * 0.5f * value * value * value * value + start;
			value -= 2;
			return -end * 0.5f * (value * value * value * value - 2) + start;
		}
		/// <summary>
		/// EaseInQuint function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseInQuint(float start, float end, float value){
			end -= start;
			return end * value * value * value * value * value + start;
		}
		/// <summary>
		/// EaseOutQuint funtion.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseOutQuint(float start, float end, float value){
			value--;
			end -= start;
			return end * (value * value * value * value * value + 1) + start;
		}
		/// <summary>
		/// EaseInOutQuint function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseInOutQuint(float start, float end, float value){
			value /= .5f;
			end -= start;
			if (value < 1) return end * 0.5f * value * value * value * value * value + start;
			value -= 2;
			return end * 0.5f * (value * value * value * value * value + 2) + start;
		}
		/// <summary>
		/// EaseInSine function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseInSine(float start, float end, float value){
			end -= start;
			return -end * Mathf.Cos(value * (Mathf.PI * 0.5f)) + end + start;
		}
		/// <summary>
		/// EaseOutSine function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseOutSine(float start, float end, float value){
			end -= start;
			return end * Mathf.Sin(value * (Mathf.PI * 0.5f)) + start;
		}
		/// <summary>
		/// EaseInOutSine function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseInOutSine(float start, float end, float value){
			end -= start;
			return -end * 0.5f * (Mathf.Cos(Mathf.PI * value) - 1) + start;
		}
		/// <summary>
		/// EaseInExpo function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseInExpo(float start, float end, float value){
			end -= start;
			return end * Mathf.Pow(2, 10 * (value - 1)) + start;
		}
		/// <summary>
		/// EaseOutExpo function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseOutExpo(float start, float end, float value){
			end -= start;
			return end * (-Mathf.Pow(2, -10 * value ) + 1) + start;
		}
		/// <summary>
		/// EaseOutExpoOver function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseOutExpoOver(float start, float end, float value){
			end -= start;
			return end * 1.2f * (-Mathf.Pow(2, -2.5f * value ) + 1) + start;
		}
		/// <summary>
		/// EaseInOutExpo function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseInOutExpo(float start, float end, float value){
			value /= .5f;
			end -= start;
			if (value < 1) return end * 0.5f * Mathf.Pow(2, 10 * (value - 1)) + start;
			value--;
			return end * 0.5f * (-Mathf.Pow(2, -10 * value) + 2) + start;
		}
		/// <summary>
		/// EaseInCirc function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseInCirc(float start, float end, float value){
			end -= start;
			return -end * (Mathf.Sqrt(1 - value * value) - 1) + start;
		}
		/// <summary>
		/// EaseOutCirc function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseOutCirc(float start, float end, float value){
			value--;
			end -= start;
			return end * Mathf.Sqrt(1 - value * value) + start;
		}
		/// <summary>
		/// EaseInOutCirc function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseInOutCirc(float start, float end, float value){
			value /= .5f;
			end -= start;
			if (value < 1) return -end * 0.5f * (Mathf.Sqrt(1 - value * value) - 1) + start;
			value -= 2;
			return end * 0.5f * (Mathf.Sqrt(1 - value * value) + 1) + start;
		}
		/// <summary>
		/// EaseInBounce function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseInBounce(float start, float end, float value){
			end -= start;
			float d = 1f;
			return end - EaseOutBounce(0, end, d-value) + start;
		}
		/// <summary>
		/// EaseOutBounce function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseOutBounce(float start, float end, float value){
			value /= 1f;
			end -= start;
			if (value < (1 / 2.75f)){
				return end * (7.5625f * value * value) + start;
			}else if (value < (2 / 2.75f)){
				value -= (1.5f / 2.75f);
				return end * (7.5625f * (value) * value + .75f) + start;
			}else if (value < (2.5 / 2.75)){
				value -= (2.25f / 2.75f);
				return end * (7.5625f * (value) * value + .9375f) + start;
			}else{
				value -= (2.625f / 2.75f);
				return end * (7.5625f * (value) * value + .984375f) + start;
			}
		}
		/// <summary>
		/// EaseInOutBounce function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseInOutBounce(float start, float end, float value){
			end -= start;
			float d = 1f;
			if (value < d* 0.5f) return EaseInBounce(0, end, value*2) * 0.5f + start;
			else return EaseOutBounce(0, end, value*2-d) * 0.5f + end*0.5f + start;
		}
		/// <summary>
		/// EaseInBack function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseInBack(float start, float end, float value){
			end -= start;
			value /= 1;
			float s = 1.70158f;
			return end * (value) * value * ((s + 1) * value - s) + start;
		}
		/// <summary>
		/// EaseOutBack function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseOutBack(float start, float end, float value){
			float s = 1.70158f;
			end -= start;
			value = (value) - 1;
			return end * ((value) * value * ((s + 1) * value + s) + 1) + start;
		}
		/// <summary>
		/// EaseInOutBack function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseInOutBack(float start, float end, float value){
			float s = 1.70158f;
			end -= start;
			value /= .5f;
			if ((value) < 1){
				s *= (1.525f);
				return end * 0.5f * (value * value * (((s) + 1) * value - s)) + start;
			}
			value -= 2;
			s *= (1.525f);
			return end * 0.5f * ((value) * value * (((s) + 1) * value + s) + 2) + start;
		}
		/// <summary>
		/// Punch function.
		/// </summary>
		/// <param name="amplitude">Amplitude.</param>
		/// <param name="value">Value.</param>
		public static float Punch(float amplitude, float value){
			float s = 9;
			if (value == 0){
				return 0;
			}
			else if (value == 1){
				return 0;
			}
			float period = 1 * 0.3f;
			s = period / (2 * Mathf.PI) * Mathf.Asin(0);
			return (amplitude * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * 1 - s) * (2 * Mathf.PI) / period));
		}
		/// <summary>
		/// EaseInElastic function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseInElastic(float start, float end, float value){
			end -= start;

			float d = 1f;
			float p = d * .3f;
			float s = 0;
			float a = 0;

			if (value == 0) return start;

			if ((value /= d) == 1) return start + end;

			if (a == 0f || a < Mathf.Abs(end)){
				a = end;
				s = p / 4;
			}else{
				s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
			}

			return -(a * Mathf.Pow(2, 10 * (value-=1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
		}		
		/// <summary>
		/// EaseOutElastic function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseOutElastic(float start, float end, float value) {
			end -= start;

			float d = 1f;
			float p = d * .3f;
			float s = 0;
			float a = 0;

			if (value == 0) return start;

			if ((value /= d) == 1) return start + end;

			if (a == 0f || a < Mathf.Abs(end)){
				a = end;
				s = p * 0.25f;
			}else{
				s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
			}

			return (a * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) + end + start);
		}		
		/// <summary>
		/// EaseInOutElastic function.
		/// </summary>
		/// <returns>The ease value.</returns>
		/// <param name="start">Start value.</param>
		/// <param name="end">End value.</param>
		/// <param name="value">Value in time (0, 1).</param>
		public static float EaseInOutElastic(float start, float end, float value) {
			end -= start;

			float d = 1f;
			float p = d * .3f;
			float s = 0;
			float a = 0;

			if (value == 0) return start;

			if ((value /= d*0.5f) == 2) return start + end;

			if (a == 0f || a < Mathf.Abs(end)){
				a = end;
				s = p / 4;
			}else{
				s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
			}

			if (value < 1) return -0.5f * (a * Mathf.Pow(2, 10 * (value-=1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
			return a * Mathf.Pow(2, -10 * (value-=1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) * 0.5f + end + start;
		}
	}
}