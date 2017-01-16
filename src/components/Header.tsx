import * as React from 'react'

import './Header.css'

export interface HeaderProps {
  onLogoClick: () => void
}

const Header = ({ onLogoClick }: HeaderProps) => (
  <div className='header'>
    <a onClick={onLogoClick}>BouncingBalls</a>
  </div>
)

export default Header