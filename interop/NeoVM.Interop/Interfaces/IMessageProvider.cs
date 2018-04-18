﻿namespace NeoVM.Interop.Interfaces
{
    public interface IMessageProvider
    {
        /// <summary>
        /// Get message for check signatures
        /// </summary>
        /// <param name="iteration">Iteration number</param>
        /// <returns>Return message data</returns>
        byte[] GetMessage(uint iteration);
    }
}