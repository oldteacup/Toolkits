using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIDTuningControl
{
    /// <summary>
    /// 控制接口， ui通过控制接口实现控制的实现
    /// </summary>
    public interface IControl
    {
        /// <summary>
        /// 当前输出值
        /// </summary>
        public double OutputValue { get; }

        /// <summary>
        /// 返回值
        /// </summary>
        public double ActualValue { get; }

        /// <summary>
        /// 目标值
        /// </summary>
        public double TargetValue { get; }

        /// <summary>
        /// 输出
        /// </summary>
        public void Output(double output);

        /// <summary>
        /// 重设
        /// </summary>
        public void Reset();
    }
}
