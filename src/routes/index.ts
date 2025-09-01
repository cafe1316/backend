// src/routes/index.ts
import { Router } from "express";
import { getPopularProductsController, getProductByIDController, searchProductsController } from "../controllers/products.controller";
import { loginController, registerNewUserController, googleLoginController } from "../controllers/users.controller";
import cartRouter from "./cart/index.route"

const router = Router();
router.use(cartRouter)

router.get("/v1/popularProducts", getPopularProductsController);

router.get("/v1/products", searchProductsController);

router.get("/v1/products/:id", getProductByIDController);

router.post("/v1/user", registerNewUserController);

router.post("/v1/user/login", loginController);

router.post("v1/user/google", googleLoginController)


export default router;