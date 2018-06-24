﻿// -----------------------------------------------------------------------
//  <copyright file="SqlServerMigrationModuleBase.cs" company="OSharp开源团队">
//      Copyright (c) 2014-2018 OSharp. All rights reserved.
//  </copyright>
//  <site>http://www.osharp.org</site>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2018-03-20 16:57</last-date>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using OSharp.Core;
using OSharp.Core.Options;
using OSharp.Core.Packs;
using OSharp.Exceptions;


namespace OSharp.Entity.SqlServer
{
    /// <summary>
    /// SqlServer数据迁移模块基类
    /// </summary>
    public abstract class SqlServerMigrationModuleBase<TDbContext> : OsharpPack
        where TDbContext : DbContext
    {
        /// <summary>
        /// 获取 模块级别
        /// </summary>
        public override PackLevel Level => PackLevel.Framework;

        /// <summary>
        /// 使用模块服务
        /// </summary>
        /// <param name="provider">服务提供者</param>
        public override void UseModule(IServiceProvider provider)
        {
            using (IServiceScope scope = provider.CreateScope())
            {
                TDbContext context = CreateDbContext(scope.ServiceProvider);
                if (context != null)
                {
                    OSharpOptions options = scope.ServiceProvider.GetOSharpOptions();
                    OSharpDbContextOptions contextOptions = options.GetDbContextOptions(context.GetType());
                    if (contextOptions != null)
                    {
                        if (contextOptions.DatabaseType != DatabaseType.SqlServer)
                        {
                            throw new OsharpException($"上下文类型“{contextOptions.DatabaseType}”不是 SqlServer 类型");
                        }
                        if (contextOptions.AutoMigrationEnabled)
                        {
                            context.CheckAndMigration();
                            int count = context.SaveChanges();
                            Debug.WriteLine($"Migration Save:{count}");
                        }
                    }
                }
            }

            IsEnabled = true;
        }

        /// <summary>
        /// 重写实现获取数据上下文实例
        /// </summary>
        /// <param name="scopedProvider">服务提供者</param>
        /// <returns></returns>
        protected abstract TDbContext CreateDbContext(IServiceProvider scopedProvider);
    }
}