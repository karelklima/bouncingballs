import { Action, ActionTypes, isAction } from 'actions'

const initialState = {
  source: null as string | null
}

export type EditorState = Readonly<typeof initialState>

export const editorReducer = (state = initialState, action: Action<ActionTypes>): EditorState => {
  if (isAction('SOURCE_CHANGED', action)) {
    console.log('SOURCE_CHANGED')
    return {
      ...state,
      source: action.payload
    }
  }
  return state
}