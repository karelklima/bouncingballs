import { Actions } from './actions'

// Define all possible actions
export type ActionTypes = keyof Actions

// Define action itself
export interface Action<T extends ActionTypes> {
  type: T
  // Type of action is mapped to payload
  payload: Actions[T]
}

export const ActionType = <T extends ActionTypes>(t: T): T => {
  return t
}

// Pack the payload and build the action
export const buildAction = <T extends ActionTypes>(type: T, payload: Action<T>['payload']): Action<T> => ({
  type, payload
})

export type BuiltActions<T> = {
  [K in keyof T]: ((payload: any) => any)
}

export const buildActionCreators = <T>(actions: T) => {
  const reducer = (memo: any, action: string) => {
    return {
      ...memo,
      [action as any as string]: (payload: any) => buildAction(
        (actions as any)[action],
        typeof payload === 'object' && payload.persist ? null : payload
      )
    }
  }
  return Object.keys(actions).reduce(reducer, {}) as BuiltActions<T>
}


// Test if anonymous action is desired kind of actions
export const isAction = <T extends ActionTypes>(k: T, a: Action<any>): a is Action<T> => {
  return a.type === k
}