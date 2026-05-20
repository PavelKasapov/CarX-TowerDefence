using VContainer;
using VContainer.Unity;

public class TowerScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<BallisticAimSolver>(Lifetime.Singleton);
        builder.Register<TurretRotation>(Lifetime.Singleton);
    }
}