import db from "../db";
import { cafe1316Products } from "../../drizzle/schema";
import { eq, like } from "drizzle-orm";

export async function getPopularProducts() {
    try {
        const result = await db.select().from(cafe1316Products).where(eq(cafe1316Products.isHot, true));
        return result;
    } catch (error) {
        console.error("查询热门产品出错:", error);
        throw error;
    }
}

export async function searchProducts(search: string) {
    try {
        const result = await db.select().from(cafe1316Products).where(like(cafe1316Products.productName, `%${search}%`));
        return result;
    } catch (error) {
        console.error("查询产品出错:", error);
        throw error;
    }
}
