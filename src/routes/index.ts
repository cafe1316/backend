// src/routes/index.ts
import { Router } from "express";
import { getPopularProductsController, getProductByIDController, searchProductsController } from "../controllers/products.controller";
import { registerNewUserController } from "../controllers/users.controller";

const router = Router();

router.get("/v1/popularProducts", getPopularProductsController);

router.get("/v1/products", searchProductsController);

router.get("/v1/products/:id", getProductByIDController);

router.post("/v1/user", registerNewUserController)

export default router;