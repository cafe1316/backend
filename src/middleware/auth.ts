import type { NextFunction, Request, Response} from "express";

export const authMiddleware = async (res: Response, req: Request, next:NextFunction) => {
    let headers = req.headers;
    let token = headers.token;
    if (!token) {
        res.send({status:403, msg:"token request!"})
        return;    
    }else{
        next();
    }
}