import * as React from 'react'
import { connect } from 'react-redux'

import HeaderContainer from 'containers/HeaderContainer'
import PlaygroundContainer from 'containers/PlaygroundContainer'
import SettingsContainer from 'containers/SettingsContainer'

import './App.css'

const App = () => (
  <div id='app-container'>
    <HeaderContainer/>
    <PlaygroundContainer/>
    <SettingsContainer/>
  </div>
)

export default App

