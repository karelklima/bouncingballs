import { combineReducers } from 'redux';

import { EditorState, editorReducer as editor } from 'reducers/editorReducer'

export type State = {
  editor: EditorState
}

export const rootReducer = combineReducers({
  editor
})