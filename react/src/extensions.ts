import { AsyncResult, Err, Ok, Result } from "functional-extensions"

type RequestType = {
    method: string
    payload?: any
}

export type ErrorType = {
    status: number
    text: string
}

let baseUrl = ""

let mapFetchError: <TE>(err: ErrorType) => TE

export const setBaseUrl = (url: string) => baseUrl = url

export function setMapFetchError<TE>(mapFunc: (err: ErrorType) => TE) {
    mapFetchError<TE> = mapFunc
}

export function request<T, TE>(request: RequestType): AsyncResult<T, TE> {
 
    const msg = {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(request.payload)
    }

    const tryFetch = async (input: RequestInfo | URL, init?: RequestInit): Promise<Result<string, ErrorType>> => {
        try {
            var response = await fetch(input, init)
            return response.status == 200
                ? new Ok<string, ErrorType>(await response.text())
                : new Err<string, ErrorType>({
                    status: response.status,
                    text: response.statusText
                })
        } catch (err) {
            return new Err<string, ErrorType>({
                status: 0,
                text: (err as any).message
            })
        }
    }

    const asyncFetch = (input: RequestInfo | URL, init?: RequestInit): AsyncResult<string, ErrorType> => 
        new AsyncResult<string, ErrorType>(tryFetch(input, init))
    
    return asyncFetch(`${baseUrl}/${request.method}`, msg)
        .mapError(mapFetchError<TE>)
        .bind(txt => Result.parseJSON<T, TE>(txt))
}