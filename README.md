# Cafe1316 Backend

基于 Bun + Express + PostgreSQL + Drizzle ORM 的后端服务。

## 🚀 技术栈

- **运行时**: [Bun](https://bun.sh) - 快速的 JavaScript 运行时
- **框架**: Express.js
- **数据库**: PostgreSQL 15
- **ORM**: Drizzle ORM
- **容器化**: Docker + Docker Compose

## 📋 前置要求

- [Docker](https://www.docker.com/get-started) 和 Docker Compose
- （可选）[Bun](https://bun.sh) - 用于本地开发

## 🏃 快速开始

### 1. 克隆项目并配置环境变量

```bash
# 克隆项目
git clone <your-repo-url>
cd backend

# 复制环境变量文件
cp .env.example .env.development
```

### 2. 启动 Docker 环境

```bash
# 启动所有服务（应用 + 数据库）
npm run docker:up

# 或者在后台运行
docker-compose up -d
```

第一次启动会自动：
- 构建应用镜像
- 拉取 PostgreSQL 镜像
- 创建数据库
- 启动服务

### 3. 运行数据库迁移

```bash
# 推送 schema 到数据库
npm run db:push

# 或者运行迁移
npm run db:migrate
```

### 4. 访问应用

- **API**: http://localhost:8080
- **健康检查**: http://localhost:8080/

## 📝 开发工作流

### 启动开发环境

```bash
# 启动所有服务（带日志输出）
npm run docker:up

# 后台启动
docker-compose up -d

# 查看日志
npm run docker:logs
```

### 代码修改

代码修改会**自动重载**（通过 volume 挂载 + `--watch` 模式）：
1. 修改 `src/` 下的任何文件
2. 保存文件
3. 应用自动重启 ✨

### 数据库操作

```bash
# 推送 schema 变更
npm run db:push

# 运行迁移
npm run db:migrate

# 启动 Drizzle Studio（数据库可视化工具）
npm run db:studio
```

### 停止服务

```bash
# 停止并删除容器
npm run docker:down

# 停止但保留容器
docker-compose stop

# 重启服务
npm run docker:restart
```

## 🔧 可用命令

### Docker 相关

| 命令 | 说明 |
|------|------|
| `npm run docker:up` | 启动所有服务 |
| `npm run docker:up:build` | 重新构建并启动 |
| `npm run docker:down` | 停止并删除容器 |
| `npm run docker:logs` | 查看实时日志 |
| `npm run docker:restart` | 重启服务 |

### 数据库相关

| 命令 | 说明 |
|------|------|
| `npm run db:push` | 推送 schema 到数据库 |
| `npm run db:migrate` | 运行数据库迁移 |
| `npm run db:studio` | 启动 Drizzle Studio |

### 本地开发（不使用 Docker）

```bash
# 安装依赖
bun install

# 启动开发服务器
bun run dev
```

## 🌍 环境变量

### `.env.development` (Docker 开发环境)

```env
DATABASE_URL=postgresql://cafe1316:cafe1316@postgres:5432/cafe1316
NODE_ENV=development
PORT=8080
JWT_SECRET=your-secret-key
```

### `.env.production` (生产环境)

```env
DATABASE_URL=postgresql://user:password@host:5432/database
NODE_ENV=production
PORT=8080
JWT_SECRET=your-production-secret
```

## 📁 项目结构

```
backend/
├── src/
│   ├── index.ts           # 应用入口
│   ├── routes/            # 路由定义
│   ├── controllers/       # 控制器
│   ├── services/          # 业务逻辑
│   ├── middleware/        # 中间件
│   └── db/                # 数据库连接
├── drizzle/
│   ├── schema.ts          # 数据库 Schema
│   └── relations.ts       # 表关系定义
├── Dockerfile             # 多阶段 Docker 构建
├── docker-compose.yml     # Docker Compose 配置
├── drizzle.config.ts      # Drizzle 配置
└── package.json
```

## 🐛 故障排查

### 容器无法启动

```bash
# 查看容器状态
docker-compose ps

# 查看详细日志
docker-compose logs app
docker-compose logs postgres
```

### 数据库连接失败

1. 确认 PostgreSQL 容器正在运行：
   ```bash
   docker-compose ps postgres
   ```

2. 检查 `.env.development` 中的 `DATABASE_URL` 是否正确

3. 确认使用的是服务名 `postgres` 而不是 `localhost`

### 端口冲突

如果 8080 或 5432 端口被占用：

```bash
# 修改 .env.development
PORT=3000
POSTGRES_PORT=5433

# 重启服务
npm run docker:down
npm run docker:up
```

### 重置数据库

```bash
# 停止服务并删除数据卷
docker-compose down -v

# 重新启动
npm run docker:up

# 重新运行迁移
npm run db:push
```

## 🔐 生产部署

### 构建生产镜像

```bash
# 构建优化的生产镜像
docker build -t cafe1316-backend:latest .

# 运行生产容器
docker run -p 8080:8080 --env-file .env.production cafe1316-backend:latest
```

### 使用 Docker Compose 部署

```bash
# 修改 docker-compose.yml 中的 volumes 配置
# 注释掉开发模式的 volume 挂载

# 启动生产环境
NODE_ENV=production docker-compose up -d
```

## 📚 相关文档

- [Bun Documentation](https://bun.sh/docs)
- [Drizzle ORM](https://orm.drizzle.team/)
- [Express.js](https://expressjs.com/)
- [Docker Compose](https://docs.docker.com/compose/)

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

## 📄 License

MIT
