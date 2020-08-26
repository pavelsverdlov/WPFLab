using System;
using System.Collections.Generic;
using System.Text;

namespace WPFLab {
    public interface IAppLogger {
        void Debug(string message);
        void Error(Exception exception);
        void Error(Exception exception, string message);
        void Error(string message);
        void Info(string message);
        void Warn(string message);
    }
}
