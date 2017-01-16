import * as React from 'react'
import { connect } from 'react-redux'

import { State } from 'reducers'
import { buildActionCreators } from 'actions'

import { getSource } from 'selectors/editorSelectors'

import './Settings.css'

export interface SettingsProps {
}

const Settings = (props: SettingsProps) => (
  <div className='settings'>
    <pre>SETTINGS</pre>
  </div>
)

export default Settings