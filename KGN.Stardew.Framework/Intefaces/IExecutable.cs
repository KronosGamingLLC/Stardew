using System;
using System.Collections.Generic;
using System.Text;

namespace KGN.Stardew.Framework.Intefaces
{
    /// <summary>
    /// Contains an Execute method
    /// </summary>
    public interface IExecutable
    {
        void Execute();
    }

    /// <summary>
    /// Contains an Execute method with a return type of T
    /// </summary>
    /// <typeparam name="T">The return type of the Execute method</typeparam>
    public interface IExecutable<T>
    {
        T Execute();
    }
}
