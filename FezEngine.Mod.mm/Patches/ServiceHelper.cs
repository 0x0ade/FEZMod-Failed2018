#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using FezEngine.Mod;
using FezEngine.Mod.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FezEngine.Tools {
    static class patch_ServiceHelper {

        public static extern void orig_AddComponent(IGameComponent component, bool addServices);
        public static void AddComponent(IGameComponent component, bool addServices) {
            IGameComponent repl;
            if (ServiceHelperHooks.ReplacementComponents.TryGetValue(component.GetType().FullName, out repl)) {
                if (repl is IServiceWrapper)
                    ((IServiceWrapper) repl).Wrap(component);
                else
                    (component as IDisposable)?.Dispose();
                component = repl;
            }

            orig_AddComponent(component, addServices);
        }

        public static extern void orig_AddService(object service);
        public static void AddService(object service) {
            object repl;
            if (ServiceHelperHooks.ReplacementServices.TryGetValue(service.GetType().FullName, out repl)) {
                if (repl is IServiceWrapper)
                    ((IServiceWrapper) repl).Wrap(service);
                else
                    (service as IDisposable)?.Dispose();
                service = repl;
            }

            orig_AddService(service);
        }

    }
}
