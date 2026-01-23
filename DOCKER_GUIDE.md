# 🚀 Docker 迁移完成 - 快速参考

## ✅ 已完成的配置

### 创建的文件
- ✅ `docker-compose.yml` - Docker Compose 配置（基于 Piggyway）
- ✅ `Dockerfile` - 多阶段优化构建
- ✅ `.dockerignore` - 优化构建上下文
- ✅ `.env.example` - 环境变量模板
- ✅ `.env.development` - 开发环境配置

### 修改的文件
- ✅ `package.json` - 添加 Docker 和数据库管理脚本
- ✅ `.gitignore` - 更新环境文件规则
- ✅ `README.md` - 完整的使用文档

## 🎯 关键差异：Piggyway vs Cafe1316

| 配置项 | Piggyway | Cafe1316 | 说明 |
|--------|----------|----------|------|
| 项目名 | piggyway | cafe1316 | ✅ 已适配 |
| 端口 | 3000 | 8080 | ✅ 已适配 |
| 启动命令 | `bun run start` | `bun run --watch src/index.ts` | ✅ 开发模式优化 |
| 数据库名 | piggyway | cafe1316 | ✅ 已适配 |
| 网络名 | piggy-network | cafe-network | ✅ 已适配 |
| 数据卷 | piggyway-pgdata | cafe1316-pgdata | ✅ 已适配 |

## 🏃 立即开始使用

### 第一次启动

```bash
# 1. 启动所有服务
npm run docker:up

# 2. 等待服务启动（约 10-30 秒）

# 3. 推送数据库 schema
npm run db:push
```

### 验证是否成功

```bash
# 检查容器状态
docker-compose ps

# 应该看到：
# cafe1316-backend    running
# cafe1316-postgres   running (healthy)

# 测试 API
curl http://localhost:8080
# 应该返回: "Hello World"
```

## 📝 常用命令速查

```bash
# 启动服务
npm run docker:up              # 前台运行（推荐开发时使用）
docker-compose up -d           # 后台运行

# 查看日志
npm run docker:logs            # 实时日志
docker-compose logs app        # 只看应用日志
docker-compose logs postgres   # 只看数据库日志

# 停止服务
npm run docker:down            # 停止并删除容器
docker-compose stop            # 只停止，不删除

# 重启服务
npm run docker:restart         # 重启所有服务
docker-compose restart app     # 只重启应用

# 数据库操作
npm run db:push                # 推送 schema
npm run db:migrate             # 运行迁移
npm run db:studio              # 可视化管理

# 重建镜像
npm run docker:up:build        # 代码或依赖变更后使用
```

## 🔧 开发模式特性

### ✨ 自动重载
代码修改后自动重启，无需手动重建：
- 修改 `src/` 下任何文件
- 保存
- 应用自动重启（约 1-2 秒）

### 📦 Volume 挂载
```yaml
volumes:
  - .:/app                    # 源代码挂载
  - /app/node_modules         # 保护 node_modules
```

### 🔍 调试
```bash
# 进入应用容器
docker-compose exec app sh

# 进入数据库容器
docker-compose exec postgres psql -U cafe1316 -d cafe1316
```

## ⚠️ 重要注意事项

### 1. 数据库连接字符串
- ✅ **Docker 环境**: `postgresql://cafe1316:cafe1316@postgres:5432/cafe1316`
  - 使用服务名 `postgres` 作为主机名
- ❌ **错误**: `postgresql://cafe1316:cafe1316@localhost:5432/cafe1316`
  - 在容器内 `localhost` 指向容器自己，不是宿主机

### 2. 环境变量优先级
```
.env 文件 < docker-compose.yml 中的 environment < 命令行环境变量
```

### 3. 数据持久化
数据存储在 Docker volume `cafe1316-pgdata` 中：
```bash
# 查看 volume
docker volume ls | grep cafe

# 删除数据（慎用！）
docker-compose down -v
```

## 🆚 与 Piggyway 的主要改进

### 1. 开发体验优化
- ✅ 使用 `--watch` 模式，代码修改实时生效
- ✅ Volume 挂载，无需重建镜像

### 2. 健康检查
- ✅ PostgreSQL 健康检查
- ✅ 应用依赖数据库健康状态才启动

### 3. 文档完善
- ✅ 详细的 README
- ✅ 环境变量示例
- ✅ 故障排查指南

## 🐛 常见问题

### Q: 端口被占用怎么办？
```bash
# 修改 .env.development
PORT=3000
POSTGRES_PORT=5433

# 重启
npm run docker:down
npm run docker:up
```

### Q: 如何重置数据库？
```bash
docker-compose down -v  # 删除数据卷
npm run docker:up       # 重新启动
npm run db:push         # 重新创建表
```

### Q: 如何查看数据库内容？
```bash
# 方法 1: Drizzle Studio
npm run db:studio

# 方法 2: psql
docker-compose exec postgres psql -U cafe1316 -d cafe1316
```

### Q: 修改依赖后需要重建吗？
```bash
# 是的，修改 package.json 后需要重建
npm run docker:up:build
```

## 🎓 下一步

1. **测试迁移**: 运行 `npm run docker:up` 确认一切正常
2. **推送 Schema**: 运行 `npm run db:push` 创建数据库表
3. **测试 API**: 访问 http://localhost:8080 测试接口
4. **查看数据**: 运行 `npm run db:studio` 可视化管理数据库

## 📚 参考资源

- [Docker Compose 文档](https://docs.docker.com/compose/)
- [Bun 文档](https://bun.sh/docs)
- [Drizzle ORM](https://orm.drizzle.team/)
- [PostgreSQL 文档](https://www.postgresql.org/docs/)

---

**准备好了吗？运行 `npm run docker:up` 开始吧！** 🚀
