using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace WPFLab {
    public static class ResourceHelper {
        public static string GetEmbeddedStringResource(Type owner, string resourcePath) {
            var assembly = owner.GetTypeInfo().Assembly;
            return GetEmbeddedStringResource(assembly, GetResourcePath(owner, resourcePath));
        }
        public static string GetEmbeddedStringResource(Assembly source, string resourcePath) {
            using (var stream = GetEmbeddedResource(source, resourcePath))
            using (var reader = new StreamReader(stream)) {
                return reader.ReadToEnd();
            }
        }
        private static string GetResourcePath(Type owner, string relativePath) {
            return owner.Namespace + '.' + relativePath;
        }
        public static Stream GetEmbeddedStreamResource(Type owner, string resourcePath) {
            return GetEmbeddedResource(owner.GetTypeInfo().Assembly, GetResourcePath(owner, resourcePath));
        }
        private static Stream GetEmbeddedResource(Assembly source, string resourcePath) {
            var stream = source.GetManifestResourceStream(resourcePath);
            if (stream == null) {
                throw new ArgumentException($"Resource [{resourcePath}] not found in assembly [{source.GetName().Name}]");
            }

            return stream;
        }
    }
}
