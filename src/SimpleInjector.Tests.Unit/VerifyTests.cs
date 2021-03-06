﻿#pragma warning disable 0618
namespace SimpleInjector.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SimpleInjector.Advanced;

    [TestClass]
    public class VerifyTests
    {
        [TestMethod]
        public void Verify_WithEmptyConfiguration_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Act
            container.Verify();
        }

        [TestMethod]
        public void Verify_SuppliedWithInvalidVerificationOptionEnum_ThrowsExpectedException()
        {
            // Arrange
            VerificationOption invalidOption = (VerificationOption)2;

            var container = new Container();

            // Act
            Action action = () => container.Verify(invalidOption);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ArgumentException>(
                "The value of argument 'option' (2) is invalid for Enum type 'VerificationOption'.",
                action);
        }

        [TestMethod]
        public void Verify_CalledMultipleTimes_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterInstance<IUserRepository>(new SqlUserRepository());

            container.Verify();

            // Act
            container.Verify();
        }

        [TestMethod]
        public void Verify_WithFailingConstructor_ReportsConcreteTypeOfFailingType()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IPlugin, FailingConstructorPlugin<Exception>>();

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                $"Creating the instance for type {typeof(FailingConstructorPlugin<Exception>).ToFriendlyName()} failed",
                action);
        }

        [TestMethod]
        public void Verify_CalledAfterGetInstance_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.RegisterInstance<IUserRepository>(new SqlUserRepository());

            container.GetInstance<IUserRepository>();

            container.Verify();
        }

        [TestMethod]
        public void Verify_WithDependantTypeNotRegistered_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // RealUserService has a constructor that takes an IUserRepository.
            container.Register<RealUserService>(Lifestyle.Singleton);

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.Throws<InvalidOperationException>(action,
                "An exception was expected because the configuration is invalid without registering an IUserRepository.");
        }

        [TestMethod]
        public void Verify_WithFailingFunc_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Register<IUserRepository>(() =>
            {
                throw new ArgumentNullException();
            });

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.Throws<InvalidOperationException>(action);
        }

        [TestMethod]
        public void Verify_RegisteredCollectionWithValidElements_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Collection.Register<IUserRepository>(new IUserRepository[] { new SqlUserRepository(), new InMemoryUserRepository() });

            // Act
            container.Verify();
        }

        [TestMethod]
        public void Verify_RegisteredCollectionWithNullElements_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<IUserRepository> repositories = new IUserRepository[] { null };

            container.Collection.Register<IUserRepository>(repositories);

            try
            {
                // Act
                container.Verify();

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (InvalidOperationException ex)
            {
                AssertThat.StringContains(
                    "One of the items in the collection for type IUserRepository is a null reference.",
                    ex.Message);
            }
        }

        [TestMethod]
        public void Verify_FailingCollection_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();

            IEnumerable<IUserRepository> repositories =
                from nullRepository in Enumerable.Repeat<IUserRepository>(null, 1)
                where nullRepository.ToString() == "This line fails with an NullReferenceException"
                select nullRepository;

            container.Collection.Register<IUserRepository>(repositories);

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.Throws<InvalidOperationException>(action);
        }

        [TestMethod]
        public void Verify_RegisterCalledWithFuncReturningNullInstances_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IUserRepository>(() => null);

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.Throws<InvalidOperationException>(action);
        }

        [TestMethod]
        public void Verify_GetRegistrationCalledOnUnregisteredAbstractType_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // This call forces the registration of a null reference to speed up performance.
            container.GetRegistration(typeof(IUserRepository));

            // Act
            container.Verify();
        }

        [TestMethod]
        public void Register_WithAnOverrideCalledAfterACallToVerify_FailsWithTheExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Options.AllowOverridingRegistrations = true;

            container.Register<IUserRepository, SqlUserRepository>();

            container.Verify();

            try
            {
                // Act
                container.Register<IUserRepository, SqlUserRepository>(Lifestyle.Singleton);

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (InvalidOperationException ex)
            {
                AssertThat.ExceptionMessageContains("The container can't be changed", ex);
            }
        }

        [TestMethod]
        public void ResolveUnregisteredType_CalledAfterACallToVerify_FailsWithTheExpectedMessage()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Verify();

            try
            {
                // Act
                container.ResolveUnregisteredType += (s, e) => { };

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (InvalidOperationException ex)
            {
                AssertThat.ExceptionMessageContains("The container can't be changed", ex);
            }
        }

        [TestMethod]
        public void ExpressionBuilding_CalledAfterACallToVerify_FailsWithTheExpectedMessage()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Verify();

            try
            {
                // Act
                container.ExpressionBuilding += (s, e) => { };

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (InvalidOperationException ex)
            {
                AssertThat.ExceptionMessageContains("The container can't be changed", ex);
            }
        }

        [TestMethod]
        public void ExpressionBuilt_CalledAfterACallToVerify_FailsWithTheExpectedMessage()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Verify();

            try
            {
                // Act
                container.ExpressionBuilt += (s, e) => { };

                // Assert
                Assert.Fail("Exception expected.");
            }
            catch (InvalidOperationException ex)
            {
                AssertThat.ExceptionMessageContains("The container can't be changed", ex);
            }
        }

        [TestMethod]
        public void Verify_RegisterCollectionCalledWithUnregisteredType_ThrowsExpectedException()
        {
            // Arrange
            string expectedException =
                "The registration for the collection of IUserRepository (i.e. IEnumerable<IUserRepository>) " +
                "is supplied with the abstract type IUserRepository, which hasn't been registered explicitly";

            var container = ContainerFactory.New();

            var types = new[] { typeof(SqlUserRepository), typeof(IUserRepository) };

            container.Collection.Register<IUserRepository>(types);

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                expectedException, action);
        }

        // See issue #690.
        [TestMethod]
        public void Verify_CollectionRegistrationPointingToAnUnregisteredAbstractType_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Register IPlugin as part of the collection, while omitting container.Register<IPlugin>
            container.Collection.Register<IPlugin>(typeof(PluginBase));

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(@"
                The registration for the collection of IPlugin (i.e. IEnumerable<IPlugin>) is supplied with
                the abstract type PluginBase, which hasn't been registered explicitly, and wasn't resolved
                using unregistered type resolution. For Simple Injector to be able to resolve this collection,
                an explicit one-to-one registration is required, e.g. Container.Register<PluginBase, MyImpl>().
                Otherwise, in case PluginBase was supplied by accident, make sure it is removed."
                .TrimInside(),
                action);
        }

        // See issue #690.
        [TestMethod]
        public void Verify_CollectionRegistrationPointingToItsAbstraction_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Register IPlugin as part of the collection, while omitting container.Register<IPlugin>
            container.Collection.Register<IPlugin>(typeof(IPlugin));

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(@"
                The registration for the collection of IPlugin (i.e. IEnumerable<IPlugin>) is supplied with
                the abstract type IPlugin, which hasn't been registered explicitly"
                .TrimInside(),
                action);
        }

        // See issue #690.
        [TestMethod]
        public void Verify_GenericCollectionRegistrationPointingToItsAbstraction_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            var types = new[] { typeof(IEventHandler<>), typeof(AuditableEventEventHandler) };

            container.Collection.Register(typeof(IEventHandler<>), types);

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(@"
                The registration for the collection of IEventHandler<AuditableEvent>
                (i.e. IEnumerable<IEventHandler<AuditableEvent>>) is supplied with the abstract type
                IEventHandler<TEvent>, which hasn't been registered explicitly"
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void Verify_OnCollection_IteratesTheCollectionOnce()
        {
            // Arrange
            const int ExpectedNumberOfCreatedPlugins = 1;
            int actualNumberOfCreatedPlugins = 0;

            var container = ContainerFactory.New();

            container.Register<PluginImpl>();

            container.Collection.Register<IPlugin>(new[] { typeof(PluginImpl) });

            container.RegisterInitializer<PluginImpl>(
                plugin => actualNumberOfCreatedPlugins++);

            // Act
            container.Verify();

            // Assert
            Assert.AreEqual(ExpectedNumberOfCreatedPlugins, actualNumberOfCreatedPlugins);
        }

        [TestMethod]
        public void Verify_MixedOneToOneAndCollectionRegistrationForSameComponent_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<PluginImpl>();

            container.Collection.Register<IPlugin>(new[] { typeof(PluginImpl) });

            // Act
            container.Verify();
        }

        [TestMethod]
        public void Verify_RootTypeCollectionWithDecoratorThatCanNotBeCreatedAtRuntime_ThrowsInvalidOperationException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Root type
            container.Collection.Register<IPlugin>(new[] { typeof(PluginImpl) });

            // FailingConstructorDecorator constructor throws an exception.
            container.RegisterDecorator(typeof(IPlugin), typeof(FailingConstructorPluginDecorator));

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.Throws<InvalidOperationException>(action);
        }

        [TestMethod]
        public void Verify_NonRootTypeCollectionWithDecoratorThatCanNotBeCreatedAtRuntime_ThrowsInvalidOperationException()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Root type
            container.Register<ServiceDependingOn<IEnumerable<IPlugin>>>();

            container.Collection.Register<IPlugin>(new[] { typeof(PluginImpl) });

            // FailingConstructorDecorator constructor throws an exception.
            container.RegisterDecorator(typeof(IPlugin), typeof(FailingConstructorPluginDecorator));

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.Throws<InvalidOperationException>(action);
        }

        [TestMethod]
        public void Verify_RegistrationWithDecoratorThatCanNotBeCreatedAtRuntimeAndBuildExpressionCalledExplicitly_ThrowsInvalidOperationException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IPlugin, PluginImpl>();

            container.RegisterDecorator(typeof(IPlugin), typeof(FailingConstructorPluginDecorator));

            container.GetRegistration(typeof(IPlugin)).BuildExpression();

            // Act
            Action action = () => container.Verify();

            // Assert
            // This test verifies a bug: Calling InstanceProducer.BuildExpression flagged the producer to be
            // skipped when calling Verify() while it was still possible that creating the instance would fail.
            AssertThat.Throws<InvalidOperationException>(action,
                "The call to BuildExpression should not trigger the verification of IPlugin to be skipped.");
        }

        [TestMethod]
        public void Verify_DecoratorWithDecorateeFactoryWithFailingDecorateeOfNonRootType_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<PluginConsumer>();

            container.Register<IPlugin, FailingConstructorPlugin<Exception>>();

            container.RegisterDecorator(typeof(IPlugin), typeof(PluginProxy), Lifestyle.Singleton);

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.Throws<InvalidOperationException>(action);
        }

        [TestMethod]
        public void Verify_RegisteredSingletonDelegateResolvingAnUnregisteredConcreteType_CreatesThatTypeJustOnce()
        {
            // Arrange
            var container = new Container();
            container.Options.ResolveUnregisteredConcreteTypes = true;

            container.Register<IPlugin>(() => container.GetInstance<PluginWithCreationCounter>(), Lifestyle.Singleton);

            // Act
            container.Verify();

            // Assert
            Assert.AreEqual(1, PluginWithCreationCounter.InstanceCount);
        }

        [TestMethod]
        public void Verify_DecoratorWithFuncDecorateeWithFailingConstructor_ThrowsTheExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Options.EnableAutoVerification = false;

            container.Register<IPlugin, FailingPlugin>();
            container.RegisterDecorator(typeof(IPlugin), typeof(PluginProxy), Lifestyle.Singleton);

            // This call will succeed, because it resolves: "new PluginProxy(() => new FailingPlugin())" and
            // that will not call the FailingPlugin constructor.
            container.GetInstance<IPlugin>();

            // Act
            // This should still throw an exception.
            Action action = () => container.Verify();

            // Assert
            AssertThat.Throws<InvalidOperationException>(action);
        }

        [TestMethod]
        public void Verify_RegisterCollectionRegistrationWithTypeReferencingAPrimitiveType_ThrowsExpectedException()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register<IPlugin>(new[] { typeof(PluginWithBooleanDependency) });

            // Act
            Action action = () => container.Verify();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(@"
                contains parameter 'isInUserContext' of type bool, which can not be used for constructor
                injection because it is a value type."
                .TrimInside(),
                action);
        }

        [TestMethod]
        public void Verify_LockedContainer_Succeeds1()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Register<IPlugin, PluginImpl>();

            container.GetInstance<Container>();

            // Act
            // This call must succeed. Many users are depending on this behavior.
            container.Verify();
        }

        [TestMethod]
        public void Verify_LockedContainerWithRegisterCollectionRegisterationForOpenGenericType_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register(typeof(IEventHandler<>), new[]
            {
                typeof(StructEventHandler),
                typeof(AuditableEventEventHandler),
                typeof(AuditableEventEventHandler<>),
            });

            container.GetInstance<Container>();

            // Act
            container.Verify();
        }

        [TestMethod]
        public void Verify_ResolvingACollectionOfSingletonsBeforeAndAfterCallingVerify_ShouldStillYieldTheSameInstance()
        {
            // Arrange
            var container = ContainerFactory.New();

            container.Collection.Register(typeof(IEventHandler<>), new Type[]
            {
                typeof(StructEventHandler),
            });

            container.Collection.Append(typeof(IEventHandler<>),
                Lifestyle.Singleton.CreateRegistration<AuditableEventEventHandler>(container));

            var handler = container.GetAllInstances<IEventHandler<AuditableEvent>>().Single();

            container.Verify();

            // Act
            var handler2 = container.GetAllInstances<IEventHandler<AuditableEvent>>().Single();

            // Assert
            Assert.AreSame(handler, handler2);
        }

        [TestMethod]
        public void VerifyOnly_WithDiagnosticWarning_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Options.SuppressLifestyleMismatchVerification = true;

            // Lifestyle Mismatch
            container.Register<ServiceWithDependency<IPlugin>>(Lifestyle.Singleton);
            container.Register<IPlugin, PluginImpl>();

            // Act
            container.Verify(VerificationOption.VerifyOnly);
        }

        [TestMethod]
        public void VerifyOnly_WithLifestyleMismathcDiagnosticWarning_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Options.SuppressLifestyleMismatchVerification = false;

            // Lifestyle Mismatch
            container.Register<ServiceWithDependency<IPlugin>>(Lifestyle.Singleton);
            container.Register<IPlugin, PluginImpl>();

            // Act
            Action action = () => container.Verify(VerificationOption.VerifyOnly);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<InvalidOperationException>(
                "lifestyle mismatch",
                action);
        }

        [TestMethod]
        public void GetInstance_WithLifestyleMismathcDiagnosticWarning_ThrowsException()
        {
            // Arrange
            var container = ContainerFactory.New();
            container.Options.EnableAutoVerification = false;
            container.Options.SuppressLifestyleMismatchVerification = false;

            // Lifestyle Mismatch
            container.Register<ServiceWithDependency<IPlugin>>(Lifestyle.Singleton);
            container.Register<IPlugin, PluginImpl>();

            // Act
            Action action = () => container.GetInstance<ServiceWithDependency<IPlugin>>();

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<ActivationException>(
                "lifestyle mismatch",
                action);
        }

        [TestMethod]
        public void VerifyAndDiagnose_WithDiagnosticWarning_ThrowsExpectedException()
        {
            // Arrange
            string expected1 = "The following diagnostic warnings were reported";
            string expected2 = "ServiceWithDependency<IPlugin> (Singleton) depends on IPlugin";
            string expected3 = "See the Error property for detailed information about the warnings.";

            var container = ContainerFactory.New();

            // Lifestyle Mismatch
            container.Register<ServiceWithDependency<IPlugin>>(Lifestyle.Singleton);
            container.Register<IPlugin, PluginImpl>();

            // Act
            Action action = () => container.Verify(VerificationOption.VerifyAndDiagnose);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<DiagnosticVerificationException>(expected1, action);
            AssertThat.ThrowsWithExceptionMessageContains<DiagnosticVerificationException>(expected2, action);
            AssertThat.ThrowsWithExceptionMessageContains<DiagnosticVerificationException>(expected3, action);
        }

        [TestMethod]
        public void VerifyAndDiagnose_WithDiagnosticWarning_ThrowsMessageReferingToTheDocumentation()
        {
            // Arrange
            string expectedMessage =
                "Please see https://simpleinjector.org/diagnostics how to fix problems and how to " +
                "suppress individual warnings.";

            var container = ContainerFactory.New();

            // Lifestyle Mismatch
            container.Register<ServiceWithDependency<IPlugin>>(Lifestyle.Singleton);
            container.Register<IPlugin, PluginImpl>();

            // Act
            Action action = () => container.Verify(VerificationOption.VerifyAndDiagnose);

            // Assert
            AssertThat.ThrowsWithExceptionMessageContains<DiagnosticVerificationException>(
                expectedMessage, action);
        }

        [TestMethod]
        public void VerifyAndDiagnose_WithDiagnosticMessage_Succeeds()
        {
            // Arrange
            var container = ContainerFactory.New();

            // Container-registered type (PluginImpl)
            container.Register<ServiceWithDependency<PluginImpl>>(Lifestyle.Transient);

            // Act
            // This should succeed, because container-registered instances have a low risk of causing errors
            // and the container will report them quite easily.
            container.Verify(VerificationOption.VerifyAndDiagnose);
        }

        public class PluginWithBooleanDependency : IPlugin
        {
            public PluginWithBooleanDependency(bool isInUserContext)
            {
            }
        }

        public sealed class FailingConstructorPluginDecorator : IPlugin
        {
            public FailingConstructorPluginDecorator(IPlugin plugin)
            {
                throw new NotImplementedException();
            }
        }

        public sealed class PluginProxy : IPlugin
        {
            public PluginProxy(Func<IPlugin> pluginFactory)
            {
            }
        }

        public sealed class FailingPlugin : IPlugin
        {
            public FailingPlugin()
            {
                throw new Exception();
            }
        }

        private sealed class PluginDecorator : IPlugin
        {
            public PluginDecorator(IPlugin plugin)
            {
            }
        }

        private sealed class FailingConstructorPlugin<TException> : IPlugin
            where TException : Exception, new()
        {
            public FailingConstructorPlugin()
            {
                throw new TException();
            }
        }

        private sealed class PluginWithCreationCounter : IPlugin
        {
            private static int counter;

            public PluginWithCreationCounter()
            {
                Interlocked.Increment(ref counter);
            }

            public static int InstanceCount
            {
                get { return counter; }
            }
        }

        private sealed class PluginConsumer
        {
            public PluginConsumer(IPlugin plugin)
            {
            }
        }
    }
}