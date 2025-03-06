using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Google.Protobuf;


namespace ChatServer.Main.MessageOperate
{
    public static class ServiceRegisteExtenstion
    {
        /// <summary>
        /// 将实现了 IProcessor<> 接口的类注册到依赖注入容器中
        /// </summary>
        /// <param name="services"></param>
        public static void AddProcessors(this IServiceCollection services)
        {
            // 获取泛型接口 IProcessor<> 的类型
            var processorType = typeof(IProcessor<>);
            // 获取当前执行程序集中的所有类型，并筛选出实现了 IProcessor<> 接口的类
            var types = Assembly.GetExecutingAssembly().GetTypes()
                                .Where(t => t.GetInterfaces()
                                             .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == processorType)
                                             && t.IsClass && !t.IsAbstract).ToList();

            // 遍历筛选出的类型
            foreach (var type in types)
            {
                // 获取该类型实现的 IProcessor<> 接口
                var interfaceType = type.GetInterfaces()
                                        .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == processorType);
                // 将接口和实现类注册到依赖注入容器中
                services.AddScoped(interfaceType, type);
            }
        }

        /// <summary>
        /// 将继承了 BusinessServer 的类注册到依赖注入容器中
        /// </summary>
        /// <param name="services"></param>
        public static void AddBusinessServers(this IServiceCollection services)
        {
            // 获取抽象类 BusinessServer 的类型
            var businessServerType = typeof(BusinessServer);
            // 获取当前执行程序集中的所有类型，并筛选出继承了 BusinessServer 的类
            var businessServerTypes = Assembly.GetExecutingAssembly().GetTypes()
                                              .Where(t => t.IsSubclassOf(businessServerType) && !t.IsAbstract).ToList();

            var iBusinessServerType = typeof(IBusinessServer);

            // 遍历筛选出的类型
            foreach (var type in businessServerTypes)
            {
                services.AddSingleton(iBusinessServerType, type);
            }
        }
    }
}
