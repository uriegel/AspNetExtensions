import { AsyncResult, Result } from "functional-extensions"

type RequestType = {
    method: string
    payload?: any
}

let baseUrl = ""

export const setBaseUrl = (url: string) => baseUrl = url

export function request<T, TE>(request: RequestType): AsyncResult<T, TE> {
 
    const msg = {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(request.payload)
    }

    return new AsyncResult<T, TE>(
        fetch(`${baseUrl}/${request.method}`, msg)
            .bind(b => b.text()
            .map(txt => Result.parseJSON<T, TE>(txt)))) 
}