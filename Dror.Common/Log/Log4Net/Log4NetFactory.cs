using System;

namespace Dror.Common.Log.Log4Net
{
    public abstract class Log4NetFactory
    {
        #region properties
        protected abstract void SetConfigFile();
        #endregion


        #region Methods
        internal log4net.ILog CreateLog(Type classType)
        {
            SetConfigFile();
            return log4net.LogManager.GetLogger(classType); 
        }

        #endregion
    }
}
