namespace agilex.fabrication
{
    /// <summary>
    /// The default fabricator used if nothing is registered for the type requested
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DefaultFabricator<T> : Fabricator<T> where T : class
    {
        /// <summary>
        /// Update the rules of fabrication for type T
        /// </summary>
        /// <param name="fabricatorConfig">The fabricator configuration object</param>
        public override void FabricationRules(FabricatorConfig fabricatorConfig)
        {
            // this is the default fabricator, if you inherit from Fabricator<T>
            // as this class does you can do the following to configure

            // set fabricator up to use constructor args
            // fabricatorConfig.SetConstructorArgs(new object[]{"arg1", "arg2"});

            // set fabricator up to use object initialisation
            // fabricatorConfig.SetObjectInitializer(instance => instance.Prop1 = "prop1 value");

            // set fabricator up to delegate construction to a Func<T>
            // fabricatorConfig.SetConstructorDelegate(() => new Instance("blah"){Prop1 = "prop1 value"});
        }
    }
}