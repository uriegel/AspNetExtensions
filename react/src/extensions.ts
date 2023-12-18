import { AsyncResult, Err, Ok, Result } from "functional-extensions"

type RequestType = {
    method: string
    payload?: any
}

let baseUrl = ""

let mapFetchError: <TE>(err: string) => TE

export const setBaseUrl = (url: string) => baseUrl = url

export function setMapFetchError<TE>(mapFunc: (err: string) => TE) {
    mapFetchError<TE> = mapFunc
}

export function request<T, TE>(request: RequestType): AsyncResult<T, TE> {
 
    const msg = {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(request.payload)
    }

    const tryFetch = async (input: RequestInfo | URL, init?: RequestInit): Promise<Result<Response, string>> => {
        try {
            return new Ok<Response, string>(await fetch(input, init))
        } catch (err) {
            return new Err<Response, string>((err as any).message)
        }
    }

    const asyncFetch = (input: RequestInfo | URL, init?: RequestInit): AsyncResult<Response, string> => 
        new AsyncResult<Response, string>(tryFetch(input, init))
    
    return asyncFetch(`${baseUrl}/${request.method}`, msg)
        .mapAsync(b => b.text())
        .mapError(mapFetchError<TE>)
        .bind(txt => Result.parseJSON<T, TE>(txt))
}