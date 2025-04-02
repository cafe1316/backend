import express from "express";
import cors from "cors";
import { Pool } from "pg";

const pool = new Pool({
    connectionString: process.env.DATABASE_URL,
});

const app = express();
const port = 8080;

app.use(cors());

app.get("/", (req, res) => {
    res.send("Hello World!");
});

// app.get("/v1/popularProducts", (req, res) => {
//     res.json({
//         products: [
//             {
//                 id: 1,
//                 name: "埃塞俄比亚耶加雪菲",
//                 image: "https://images.unsplash.com/photo-1559056199-641a0ac8b55e?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=1440&q=80",
//                 description: "花香果酸，清爽回甘",
//                 price: 128
//             },
//         ],
//     });
// });

app.get("/v1/popularProducts", async (req, res) => {
    try {
        const result = await pool.query(
            "SELECT * FROM cafe1316_product WHERE is_hot = true;"
        );
        
        res.json({
            products: result.rows,
        });
    } catch (error) {
        console.error("数据库查询出错:", error);
        res.status(500).json({ error: "服务器内部错误" });
    }
});

app.listen(port, () => {
    console.log(`Listening on port ${port}...`);
});