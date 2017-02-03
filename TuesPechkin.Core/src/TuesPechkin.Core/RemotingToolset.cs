using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace TuesPechkin
{
    /// <summary>
    /// Deployments loaded with this class must be marked Serializable.
    /// </summary>
    /// <typeparam name="TToolset">The type of toolset to manage remotely.</typeparam>
    public sealed class RemotingToolset<TToolset> : NestingToolset
        where TToolset : MarshalByRefObject, IToolset, new()
    {
        public RemotingToolset(IDeployment deployment)
        {
            if (deployment == null)
            {
                throw new ArgumentNullException("deployment");
            }

            Deployment = deployment;
        }

        public override void Load(IDeployment deployment = null)
        {
            if (Loaded)
            {
                return;
            }

            if (deployment != null)
            {
                Deployment = deployment;
            }

            var handle = Activator.CreateInstance(typeof(TToolset));

            NestedToolset = handle as IToolset;
            NestedToolset.Load(Deployment);
            Deployment = NestedToolset.Deployment;

            Loaded = true;
        }

        public override event EventHandler Unloaded;

        public override void Unload()
        {
            if (Loaded)
            {
                //TearDownAppDomain(null, EventArgs.Empty);
            }
        }
    }
}