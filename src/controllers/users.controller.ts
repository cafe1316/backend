import type { Request, Response } from "express";
import { googleLogin, login, registerNewUser } from "../services/userService";


export const registerNewUserController = async (req: Request, res: Response) => {
    const body = req.body;
    // body is an object with key & value
    // username, email, password, confirmPassword
    try {
        await registerNewUser(body)
    } catch (error: any) {
        console.log(error);
        res.json({status: 400, msg: error.message})
        return;
    }
    res.json({status: 200, data: "OK"})
}

export const loginController = async (req:Request, res: Response) => {
    const body = req.body;
    try {
        let token = await login(body)
        res.json({status: 200, token})
    } catch (error: any) {
        // console.log(error.message);
        res.json({status: 400, msg: error.message})
        return;
    }
}

export const googleLoginController = async (req:Request, res: Response) => {
    const {idToken} = req.body;
    if (!idToken){
        res.json({status: 400, msg:"missing google id token"});
        return;
    }

    try{
        const result = await googleLogin(idToken);
        res.json({status:200, token:result.token, user: result.user})
    }catch (error: any){
        res.json({status: 400, msg: error.message || "google login failed"});
        return;
    }
}