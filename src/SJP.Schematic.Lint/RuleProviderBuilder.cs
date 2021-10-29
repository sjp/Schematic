using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint
{
    /// <summary>
    /// A builder that is used to cumulatively build up a set of rule providers to build a final rule provider instance.
    /// </summary>
    /// <seealso cref="IRuleProviderBuilder"/>
    public sealed class RuleProviderBuilder : IRuleProviderBuilder
    {
        private readonly IEnumerable<IRuleProvider> _ruleProviders;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleProviderBuilder"/> class.
        /// </summary>
        public RuleProviderBuilder()
        {
            _ruleProviders = Array.Empty<IRuleProvider>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleProviderBuilder"/> class.
        /// </summary>
        /// <param name="ruleProviders">Rule providers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="ruleProviders"/> is <c>null</c>.</exception>
        private RuleProviderBuilder(IEnumerable<IRuleProvider> ruleProviders)
        {
            _ruleProviders = ruleProviders ?? throw new ArgumentNullException(nameof(ruleProviders));
        }

        /// <summary>
        /// Adds a rule provider.
        /// </summary>
        /// <param name="ruleProvider">A rule provider.</param>
        /// <returns>A rule provider builder that now also builds rules from <paramref name="ruleProvider"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ruleProvider"/> is <c>null</c>.</exception>
        public IRuleProviderBuilder AddRuleProvider(IRuleProvider ruleProvider)
        {
            if (ruleProvider == null)
                throw new ArgumentNullException(nameof(ruleProvider));

            var ruleProviders = _ruleProviders.Append(ruleProvider).ToList();
            return new RuleProviderBuilder(ruleProviders);
        }

        /// <summary>
        /// Adds a rule provider.
        /// </summary>
        /// <typeparam name="T">A rule provider type.</typeparam>
        /// <returns>A rule provider builder that now also builds rules from the rule provider declared in <typeparamref name="T"/>.</returns>
        public IRuleProviderBuilder AddRuleProvider<T>() where T : IRuleProvider, new()
        {
            var ruleProviders = _ruleProviders.Append(new T()).ToList();
            return new RuleProviderBuilder(ruleProviders);
        }

        /// <summary>
        /// Constructs an <see cref="IRuleProvider" /> based on the rule providers that have been added.
        /// </summary>
        /// <returns>A rule provider.</returns>
        public IRuleProvider Build()
        {
            return _ruleProviders.Empty()
                ? new EmptyRuleProvider()
                : new CompositeRuleProvider(_ruleProviders);
        }
    }
}
