import express from "express";
import cors from "cors";
import routes from "./routes";

const app = express();
const port = 8080;

// 中间件
app.use(cors());
app.use(express.json());

app.get("/", (req, res) => {
    res.send("Hello World");
});

// 注册路由
app.use(routes);

// 启动服务器
app.listen(port, () => {
    console.log(`服务器运行在 http://localhost:${port}`);
});