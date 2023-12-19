import { ErrorType, jsonPost, setBaseUrl } from 'functional-extensions'
import './App.css'

setBaseUrl("http://localhost:2000/requests")

type Request2 = {
	name: string
	id: number
}

type Result = {
	result: string
	id: number
}

interface ErrorResult extends ErrorType {
	msg?: string
	code?: number
}

const makeRequest1Type = () => ({ method: "req1" })
const makeRequest2Type = (payload: Request2) => ({
    method: "req2",
	payload
})
const makeRequest3Type = () => ({ method: "req3"})
const makeRequest6Type = () => ({ method: "req6" })
const makeRequest7Type = () => ({ method: "req7" })

function App() {
	const onRequest = async () => {
		const res = jsonPost<Result, ErrorResult>(makeRequest1Type())
		console.log(await res.toResult())
	}

	const onRequest2 = async () => {
		const res = jsonPost<Result, ErrorResult>(makeRequest2Type({ name: "Uwe Riegel", id: 9865 }))
		console.log(await res.toResult())
	}

	const onRequest3 = async () => {
		const res = jsonPost<Result, ErrorResult>(makeRequest3Type())
		console.log(await res.toResult())
	}
	
	const onRequest4 = async () => {
		setBaseUrl("http://localhost:2001/requests")
		const res = jsonPost<Result, ErrorResult>(makeRequest1Type())
		console.log(await res.toResult())
		setBaseUrl("http://localhost:2000/requests")
	}

	const onRequest5 = async () => {
		alert("call this site from 'http://localhost:2001'")
		const res = jsonPost<Result, ErrorResult>(makeRequest1Type())
		console.log(await res.toResult())
	}

	const onRequest6 = async () => {
		const res = jsonPost<Result, ErrorResult>(makeRequest6Type())
		console.log(await res.toResult())
	}

	const onRequest7 = async () => {
		const res = jsonPost<Result, ErrorResult>(makeRequest7Type())
		console.log(await res.toResult())
	}

	return (
		<div>
			<button onClick={onRequest}>Request</button>
			<button onClick={onRequest2}>Request 2</button>
			<button onClick={onRequest3}>Request Error</button>
			<button onClick={onRequest4}>No Connection</button>
			<button onClick={onRequest5}>CORS problem</button>
			<button onClick={onRequest6}>Wrong method</button>
			<button onClick={onRequest7}>Server exception</button>
		</div>
	)
}

export default App
