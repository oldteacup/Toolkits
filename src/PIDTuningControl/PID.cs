using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIDTuningControl
{
    /// <summary>
    /// PID
    /// </summary>
    /// <param name="KP">比例系数</param>
    /// <param name="KI">积分系数</param>
    /// <param name="KD">微分系数</param>
    public class PID
    {
        private double _errSum;

        private double _errLast;
        private double _errSecondLast;

        /// <summary>
        /// PID控制方式
        /// </summary>
        public ControlType PIDType { get; set; }

        public double KP { get; set; }
        public double KI { get; set; }
        public double KD { get; set; }

        public PID(double kp, double ki, double kd)
        {
            KP = kp;
            KI = ki;
            KD = kd;
        }

        /// <summary>
        /// PID 计算获得输出值
        /// </summary>
        /// <param name="target">目标值</param>
        /// <param name="present">当前值</param>
        /// <returns></returns>
        public double Calculate(double target, double present)
        {
            if (KP == 0 && KI == 0 && KD == 0)
            {
                return target;
            }

            // 计算当前误差
            double errNow = target - present;

            double output;
            if (PIDType == ControlType.Positional)
            {
                // 位置式PID控制
                double errSum = _errSum + errNow;
                output = KP * errNow + KI * errSum + KD * (errNow - _errLast);
                _errSum = errSum;
            }
            else
            {
                // 增量式PID控制
                output = KP * (errNow - _errLast) + KI * errNow + KD * (errNow - 2 * _errLast + _errSecondLast);
            }

            // 保存误差和
            _errLast = errNow;

            // 保存当前值
            _errSecondLast = _errLast;
            _errLast = errNow;

            return output;
        }

        /// <summary>
        /// 重置
        /// </summary>
        public void Reset()
        {
            _errSum = 0;

            _errLast = 0;
            _errSecondLast = 0;
        }


        /// <summary>
        /// 控制方式的枚举类型
        /// </summary>
        public enum ControlType
        {
            /// <summary>
            /// 位置式
            /// </summary>
            Positional,
            /// <summary>
            /// 增量式
            /// </summary>
            Incremental
        }
    }
}
