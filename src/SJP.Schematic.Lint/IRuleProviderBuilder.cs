namespace SJP.Schematic.Lint
{
    /// <summary>
    /// A builder that is used to cumulatively build up a set of rule providers to build a final rule provider instance.
    /// </summary>
    /// <seealso cref="IRuleProvider"/>
    public interface IRuleProviderBuilder
    {
        /// <summary>
        /// Adds a rule provider.
        /// </summary>
        /// <param name="ruleProvider">A rule provider.</param>
        /// <returns>A rule provider builder that can be used to construct a rule provider.</returns>
        IRuleProviderBuilder AddRuleProvider(IRuleProvider ruleProvider);

        /// <summary>
        /// Adds a rule provider.
        /// </summary>
        /// <typeparam name="T">A rule provider</typeparam>
        /// <returns>A rule provider builder that can be used to construct a rule provider.</returns>
        IRuleProviderBuilder AddRuleProvider<T>() where T : IRuleProvider, new();

        /// <summary>
        /// Constructs an <see cref="IRuleProvider"/> based on the rule providers that have been added.
        /// </summary>
        /// <returns>A rule provider.</returns>
        IRuleProvider Build();
    }
}
