import { ErrorType, Nothing, jsonPost, setBaseUrl } from 'functional-extensions'
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
const makeRequest8Type = () => ({ method: "req8" })

function App() {
	const onRequest = () => 
		jsonPost<Result, ErrorResult>(makeRequest1Type())
		.match(console.log, console.log)

	const onRequest2 = () => 
		jsonPost<Result, ErrorResult>(makeRequest2Type({ name: "Uwe Riegel", id: 9865 }))
		.match(console.log, console.log)
	

	const onRequest3 = () => 
		jsonPost<Result, ErrorResult>(makeRequest3Type())
		.match(console.log, console.log)
	
	const onRequest4 = () => {
		setBaseUrl("http://localhost:2001/requests")
		jsonPost<Result, ErrorResult>(makeRequest1Type())
		.match(console.log, console.log)
		setBaseUrl("http://localhost:2000/requests")
	}

	const onRequest5 = () => {
		alert("call this site from 'http://localhost:2001'")
		jsonPost<Result, ErrorResult>(makeRequest1Type())
		.match(console.log, console.log)
	}

	const onRequest6 = () => 
		jsonPost<Result, ErrorResult>(makeRequest6Type())
		.match(console.log, console.log)
	

	const onRequest7 = () => 
		jsonPost<Result, ErrorResult>(makeRequest7Type())
		.match(console.log, console.log)	
	
	const onRequest8 = () => 
		jsonPost<Nothing, ErrorResult>(makeRequest8Type())
		.match(() => console.log("nothing OK"), e => console.log("nothing Error", e))

	return (
		<div>
			<button onClick={onRequest}>Request</button>
			<button onClick={onRequest2}>Request 2</button>
			<button onClick={onRequest3}>Request Error</button>
			<button onClick={onRequest4}>No Connection</button>
			<button onClick={onRequest5}>CORS problem</button>
			<button onClick={onRequest6}>Wrong method</button>
			<button onClick={onRequest7}>Server exception</button>
			<button onClick={onRequest8}>Result nothing</button>
		</div>
	)
}

export default App
