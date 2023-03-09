// Required modules
import { h, Component, render } from 'preact';
import { useState, useCallback } from 'preact/hooks';
import htm from 'htm';
import { tracer, api } from './trace.js';

import * as config from './config.js';

// Used to create inline HTML templates 
const html = htm.bind(h);

// Declaring this a way to export function from App,
// there maybe a better way.
let onStart;

// App is the main application component inserted to index.html.
function App() {
    // Constants used for application state
    const [todos, setTodos] = useState([]);
    const [lastTraceId, setTraceId] = useState("");
    const [serviceNotReady, setServiceNotReady] = useState(false);

    let newTodoName = ""

    function callApi(method, endpoint, data) {
        // Set traceId in UI to current trace.id
        setTraceId(api.trace.getActiveSpan().spanContext().traceId);

        return fetch(`${config.API_URL}${endpoint}`, {
            method: method,
            body: (data == null) ? null : JSON.stringify(data),
            headers: {
                "Content-Type": "application/json",
            },
            redirect: "manual"
        });
    }


    onStart = function () {
        const span = tracer.startSpan("On Start");
        return api.context.with(api.trace.setSpan(api.context.active(), span), async () => {

            return fetch(`${config.API_URL}/health/live`)
                .then(response => {
                    if (response.status != 200) {
                        setServiceNotReady(true);
                        span.setStatus({ code: api.SpanStatusCode.ERROR, message: "Service Not Ready" });
                    }
                    refreshTodos()
                })
                .then(() => {
                    span.end();
                });
        });
    }

    function refreshTodos() {
        const span = tracer.startSpan("Refresh ToDos");
        return api.context.with(api.trace.setSpan(api.context.active(), span), async () => {
            return callApi("GET", "/api/Todo", null)
                .then(response => response.json())
                .then(todos => {
                    setTodos(todos);
                    span.end();
                })
                .catch(error => {
                    span.setStatus({ code: api.SpanStatusCode.ERROR, message: error });
                    span.end();
                });
        });
    }

    function addTodo() {
        const span = tracer.startSpan("Add New ToDo");
        return api.context.with(api.trace.setSpan(api.context.active(), span), async () => {

            const newTodo = { id: 0, name: newTodoName, isComplete: false };
            return callApi("POST", "/api/Todo", newTodo)
                .then(response => {
                    return refreshTodos();
                })
                .then(() => {
                    span.end();
                })
                .catch(error => {
                    span.setStatus({ code: api.SpanStatusCode.ERROR, message: error });
                    span.end();
                });
        });
    }

    function updateTodoIsComplete(todo, isComplete) {
        const span = tracer.startSpan("Update ToDo");
        return api.context.with(api.trace.setSpan(api.context.active(), span), async () => {

            todo.isComplete = isComplete;
            return callApi("PUT", `/api/Todo/${todo.id}`, todo)
                .then(response => {
                    return refreshTodos();
                })
                .then(() => {
                    span.end();
                })
                .catch(error => {
                    span.setStatus({ code: api.SpanStatusCode.ERROR, message: error });
                    span.end();
                });
        });
    }

    function deleteTodo(todoId) {
        const span = tracer.startSpan("Delete ToDo");
        return api.context.with(api.trace.setSpan(api.context.active(), span), async () => {

            return callApi("DELETE", `/api/Todo/${todoId}`, null)
                .then(response => {
                    return refreshTodos();
                })
                .then(() => {
                    span.end();
                })
                .catch(error => {
                    span.setStatus({ code: api.SpanStatusCode.ERROR, message: error });
                    span.end();
                });
        });
    }

    function onInputTodoName(e) {
        newTodoName = e.target.value;
    }

    class Header extends Component {
        render() {
            return html`<header>
            <h1>Todo App</h1>
        </header>`
        }
    }

    class TodoList extends Component {
        render() {
            return html`<ul>
            ${todos.map(todo => html`
            <li key=${todo.id}>
                <input type="checkbox" checked=${todo.isComplete} />
                <label for="todo-${todo.id}" onClick=${() => updateTodoIsComplete(todo, !todo.isComplete)}></label>
                ${todo.name}
                <button onClick=${() => deleteTodo(todo.id)} id="Delete Todo">Delete</button>
            </li>
            `)}
        </ul>`
        }
    }

    class NewTodo extends Component {
        render() {
            return html`<div class="newTodo">
        <input type="text" onInput=${onInputTodoName} placeholder="Enter text for new todo" />
        <button onClick=${addTodo}>Add Todo</button>
        </div>`
        }
    }

    class LastTrace extends Component {
        render() {
            return html`<div>
            <label for="traceid-input">TraceId: </label>
            <input type="text" value="${lastTraceId}" placeholder="Generated with request" readonly="true" />
            </div>`
        }
    }

    class ServiceNotReady extends Component {
        render() {
            return html`<li>
                <div style="color:red">Service not ready.</div>
                <div>Check that the services in the cluster are running correctly. After intializing the 
                cluster, <b>deploy/appregister.sh</b> must be run to configure Azure AD App Registrations.</div>
            </li>`
        }
    }

    return (
        html`<div>
        <${Header} />
        <${serviceNotReady && ServiceNotReady} />
        <${TodoList} />
        <${NewTodo} />
        <${LastTrace} />
        </div>
        `
    );
}

class OtherLinks extends Component {
    render() {
        return html`<h2>Other links</h2>
        <ul>
          <li><a href="${config.API_URL}/swagger/index.html" target="_blank">TodoAPI Swagger</a></li>
          <li><a href="${config.GRAFANA_URL}" target="_blank">Grafana</a></li>
          <li><a href="${config.JAEGER_URL}" target="_blank">Jaeger</a></li>
        </ul>`
    }
}

render(html`<${App} />`, document.getElementById('app'));
render(html`<${OtherLinks} />`, document.getElementById('links'));

onStart();