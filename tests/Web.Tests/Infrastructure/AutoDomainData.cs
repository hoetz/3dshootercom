using Ploeh.AutoFixture.Xunit2;
using Ploeh.AutoFixture;
using NSubstitute;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Mvc.Abstractions;
using Ploeh.AutoFixture.AutoNSubstitute;
using Microsoft.AspNet.Mvc.ViewFeatures;

public class AutoDomainDataAttribute : AutoDataAttribute
    {
        public AutoDomainDataAttribute()
            : base(new Fixture()
            .Customize(new DomainCustomization())
            .Customize(new MvcCustomization()))
        {
       
        }

        public class DomainCustomization : CompositeCustomization
        {
            public DomainCustomization()
                : base(
                    new AutoNSubstituteCustomization())
            {
            }
        }


        private class MvcCustomization : ICustomization
        {
            public void Customize(IFixture fixture)
            {

                fixture.Register<ViewDataDictionary>(() => new ViewDataDictionary(new EmptyModelMetadataProvider(),
                    new ModelStateDictionary()));
                
                fixture.Register<ActionContext>(() => new ActionContext(Substitute.For<HttpContext>(), new RouteData(), new ActionDescriptor()));
                
            }
        }
    }
