using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace WPFLab {
    public abstract class MapperService {
        readonly Mapper mapper;
        public MapperService() {
            var configuration = new MapperConfiguration(cfg => {
                RegisterMaping(cfg);
            });

            mapper = new Mapper(configuration);
        }

        protected abstract void RegisterMaping(IMapperConfigurationExpression cfg);

        public TOut Map<TIn, TOut>(in TIn _in) {
            return mapper.Map<TIn, TOut>(_in);
        }
    }
}
