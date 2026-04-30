using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameScope : LifetimeScope
{
    /*[SerializeField] Transform _uISettingsPanel;
    [SerializeField] Transform _featureModulesPanel;
    [SerializeField] Transform _poolTransform;
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(Camera.main.transform).Keyed("CameraTransform");
        builder.RegisterInstance(Camera.main);

        builder.RegisterInstance(_helpWindow);
        builder.RegisterInstance(_poolTransform).Keyed("SettingControlPool");
        builder.RegisterInstance(_featureModulesPanel).Keyed("FeatureModulesPanel");
        builder.RegisterInstance(_uISettingsPanel).Keyed("UISettingsPanel");

        builder.RegisterMonobehaviourFactory<SettingsControlModule>(_modulePrefab);
        builder.RegisterMonobehaviourFactory<FeatureModuleGroup>(_featureGroupPrefab);
        builder.RegisterMonobehaviourFactory<FeatureButton>(_featureButtonPrefab);
        builder.RegisterMonobehaviourFactory<FeatureButton>(_featureHelpButtonPrefab).Keyed("HelpButtonFactory");

        builder.RegisterInstance(_floatControlPrefab).As<BaseSettingControl>();

        builder.Register<SettingControlProvider>(Lifetime.Singleton);
        builder.RegisterEntryPoint<FeatureManager>(Lifetime.Singleton).AsSelf();
        builder.Register<SettingsRegistry>(Lifetime.Singleton);

        builder.RegisterEntryPoint<UISettingsPanelPresenter>(Lifetime.Singleton);
        builder.RegisterEntryPoint<FeatureModulesPanelPresenter>(Lifetime.Singleton);
        builder.RegisterEntryPoint<HelpWindowPresenter>(Lifetime.Singleton);

        builder.Register<FeatureRegistry>(Lifetime.Singleton).AsImplementedInterfaces();
    }*/
}

/*public static class VContainerExtensions
{
    public static RegistrationBuilder RegisterMonobehaviourFactory<T>(this IContainerBuilder builder, T prefab, Lifetime lifetime = Lifetime.Singleton)
        where T : MonoBehaviour
    {
        if (prefab == null) throw new ArgumentNullException(nameof(prefab));
        return builder.Register(container => new SimpleMonobehaviourFactory<T>(container, prefab), lifetime);
    }
}*/
