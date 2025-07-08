import db from "../db";
import { cafe1316Products } from "../../drizzle/schema";
import { eq,like,asc,desc,and,gte,lte,SQL,between} from "drizzle-orm";

export async function getPopularProducts() {
    try {
        const result = await db.select().from(cafe1316Products).where(eq(cafe1316Products.isHot, true));
        return result;
    } catch (error) {
        console.error("查询热门产品出错:", error);
        throw error;
    }
}

export async function searchProducts({
    name,
    baked,
    priceFrom,
    priceTo,
    sort = "asc",
    page = 1,
    num = 20,
}:{
    name?: string;
    baked?: string;
    priceFrom?:string;
    priceTo?: string;
    sort?: "asc" | "desc";
    page?: number;
    num?: number;
}) {
    const offset = (page - 1) * num;

    /*
    ================================================
    Filters Logic
    ================================================
    */
    const filterConditions: SQL[]= [];

    // name is like
    if (name) {
        filterConditions.push(like(cafe1316Products.productName, `%${name}%`));
    }

    // bake [中浅烘焙、深烘焙]
    if (baked) {
        filterConditions.push(eq(cafe1316Products.roasting, baked));
    }

    if (priceFrom !== undefined && priceTo !== undefined) {
        filterConditions.push(between(cafe1316Products.originalPrice, priceFrom, priceTo));
    }else if (priceFrom !== undefined) {
        filterConditions.push(gte(cafe1316Products.originalPrice, priceFrom))
    }else if (priceTo !== undefined) {
        filterConditions.push(lte(cafe1316Products.originalPrice, priceTo))
    }

    /*
    ================================================
    Combine Conditions
    ================================================
    */
    let combinedConditions: SQL | undefined = undefined;
    if (filterConditions.length > 0) {
        combinedConditions = and(...filterConditions);
    }

      /*
    ================================================
    Count Query for total
    ================================================
    */
    const totalItems = await db.$count(cafe1316Products, combinedConditions);
    const totalPages = Math.ceil(totalItems / num);

    if (totalItems === 0) {
        return {
          currentPage: page,
          perPage: num,
          totalItems,
          totalPages,
          products: [],
        };
    }

    /*
    ================================================
    Final Data Query
    ================================================
    */
    const products = await db
    .select()
    .from(cafe1316Products)
    .where(combinedConditions)
    .orderBy(sort === "desc" ? desc(cafe1316Products.originalPrice) : asc(cafe1316Products.originalPrice))
    .limit(num)
    .offset(offset);
    
    return {
        currentPage: page,
        perPage: num,
        totalItems,
        totalPages,
        products,
    };
}
    
 

export async function getPaginatedProducts(page=1, num=20) {
    try {
        const count = await db.$count(cafe1316Products);
        const totalItems = Number(count);
        const totalPages = Math.ceil(totalItems / num);
        if (page < 1){
            throw new Error("页码最小为1");
        }
        if (totalPages > 0 && page > totalPages){
            throw new Error(`页码超出范围，最大页数为 ${totalPages}`);
        }

        const offset = (page - 1) * num;
        const result = await db.select().from(cafe1316Products).orderBy(asc(cafe1316Products.id)).limit(num).offset(offset);

        return{currentPage: page,
            perPage: num,
            totalItems,
            totalPages,
            result};
    } catch (error) {
        console.error("分页结果显示出错", error);
        throw error;
    }
}

export async function getProductByID(id:number) {
    try{
        const result = await db.select().from(cafe1316Products).where(eq(cafe1316Products.id, id)).limit(1);
        return result[0] || null;
    }catch (error){
        console.error("查询口味特点出错", error);
        throw error;
    }
}