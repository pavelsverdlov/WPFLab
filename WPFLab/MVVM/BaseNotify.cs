﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;

namespace WPFLab.MVVM {
    public abstract class BaseNotify : ObservableObject {
        private bool isBusy;
        readonly Cursor currentCursor;
        public virtual bool IsBusy { 
            get => isBusy;
            set {
                isBusy = value;
                Mouse.OverrideCursor = value ? Cursors.Wait : currentCursor;
            }
        }
        
        protected BaseNotify() {
            currentCursor = Mouse.OverrideCursor;
            //IsBusy = true;            
        }

        protected void RefreshProperties() {
            CommandManager.InvalidateRequerySuggested();
            SetPropertyChanged();
        }

        protected bool Update<T>(ref T currentValue, T newValue, [CallerMemberName] string propertyName = "") {
            if (EqualityComparer<T>.Default.Equals(currentValue, newValue)) {
                return false;
            }
            currentValue = newValue;
            SetPropertyChanged(propertyName);
            //return PropertyChanged.SetProperty(this, ref currentValue, newValue, propertyName);
            return true;
        }

        /// <summary>
        /// the same as OnPropertyChanged
        /// </summary>
        /// <param name="propertyName"></param>
        protected void SetPropertyChanged([CallerMemberName] string propertyName = "") {
            OnPropertyChanged(propertyName);
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void NotifyOfPropertyChange<TProperty>(Expression<Func<TProperty>> property) {
            SetPropertyChanged(GetMemberName(property));
        }

        static string GetMemberName<TProperty>(Expression<Func<TProperty>> property) {
            return GetMemberInfo(property).Name;
        }
        static MemberInfo GetMemberInfo(Expression expression) {
            var lambdaExpression = (LambdaExpression)expression;
            return
                (!(lambdaExpression.Body is UnaryExpression)
                    ? (MemberExpression)lambdaExpression.Body : (MemberExpression)((UnaryExpression)lambdaExpression.Body).Operand)
                    .Member;
        }


        public virtual void OnLoaded() {
            IsBusy = false;
        }
        public virtual void OnUnloaded() { }
    }
}
